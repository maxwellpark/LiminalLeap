# Secrets, licensing and machine setup

How the credentials this project needs are stored and reached, on each machine that
needs them, and what is still not set up. Written 2026-11-24. Each step is marked
**verified** or **unverified**; tick things off here as they get done.

## The model

Every secret lives in 1Password, in one vault. The repo holds only `op://` references to
it (`scripts/itch.env`, `scripts/unity.env`), which are safe to commit. A machine
reaches the vault one of two ways:

- **The Mac, interactively:** the 1Password desktop app with CLI integration, unlocked by
  Touch ID. Nothing stored.
- **Anywhere unattended** (a cloud session, GitHub Actions): a 1Password **service
  account** token in `OP_SERVICE_ACCOUNT_TOKEN`. That token is the only secret stored
  outside 1Password, and it can only read the one vault.

`op run --env-file=<file> -- <command>` resolves the references into the child process
only, and masks them in its output.

**The boundary, stated plainly:** this keeps credentials out of the repo, chat, shell
history and environment settings, and makes revocation one action. It is not a sandbox.
Anything running where the token is can resolve what the token can read. The containment
is the vault holding only automation credentials, each revocable at its source.

## What needs a secret

| Secret | 1Password item | Used by |
|---|---|---|
| itch API key | `Deploy / itch butler / credential` | `scripts/deploy-itch.sh`, deploy workflow |
| Unity login | `Deploy / Unity CI / username`, `password` | `scripts/unity-activate.sh`, test and deploy workflows |
| Service account token | not in the vault | cloud environment variable, GitHub Actions secret |

## 1. The 1Password vault and service account

1. Create a vault named **Deploy**. Keep only automation credentials in it.
2. Add the itch key (see the comment in `scripts/itch.env` for the exact command).
3. Add the Unity login as a Login item titled **Unity CI** (see `scripts/unity.env`).
   Use the dedicated account from section 2, not a personal one.
4. Create a service account with **read** access to **Deploy** only, no other vaults.
   Save its token in the 1Password app itself; it is shown once.
5. Check the references from any machine with the token set:

   ```
   scripts/deploy-itch.sh --check
   scripts/unity-activate.sh --check
   ```

   Both print only whether each value resolved and its length.

To rotate: create a new token, update the places in section 4, then delete the old one.
To cut everything off at once: delete the service account.

## 2. A Unity account for automation

Unity Personal has no service accounts; a licence belongs to a Unity ID. So:

- Create a Unity ID used only for automation, with its own email address, and accept
  Personal on it. It can then be revoked or have its password changed without touching
  anyone's own account. Check Unity's terms for your situation before relying on it.
- **Two-factor:** the licensing client's command line takes a username and password and
  has no way to take a one-time code. Expect activation to fail on an account with
  two-factor turned on. **Unverified**; confirm on first activation.
- Unity no longer supports manual (`.ulf` file) activation for Personal, so
  `UNITY_LICENSE` is not an option for this account. It is only useful with a paid seat.
- The paid alternative built for this is Unity Build Server with a self-hosted licensing
  server. Unity does not officially support running that in cloud or container
  environments, and it is more than this project needs.

## 3. Activating Unity on a machine

```
scripts/unity-activate.sh            activate, falling back to Personal if there's no seat
scripts/unity-activate.sh --status   what this machine is licensed for
scripts/unity-activate.sh --return   hand the activation back
```

It drives the licensing client bundled with the editor (`--activate-all
--include-personal`), finds it on macOS or Linux, and reads the login through `op run`.
The licensing client only takes the login as arguments, so it is visible in the process
list for the second or so activation runs.

Run `--return` before a machine is thrown away, so activations don't pile up on the
account. Personal activations expire after a while; rerun the script when Unity reports
no licence (batch mode exits with code 198).

Status: script logic **verified** against stand-ins for `op` and the licensing client,
and `--status` against the real client. A real activation is **unverified**.

## 4. Per machine

### The Mac

1. `brew install --cask 1password-cli`, and `scripts/install-butler.sh` for butler.
2. 1Password app: Settings, Developer, "Integrate with 1Password CLI".
3. Unity Hub signs in as you for interactive work, which is fine. The automation account
   is for machines nobody sits at.

### Cloud sessions (Claude Code on the web)

Sessions for this repo run in a personal cloud environment set up for it, with these
hosts allowed beyond the defaults:

```
download.unity3d.com         editor and module downloads
packages.unity.com           package registry
download.packages.unity.com  package registry downloads
packages-v2.unity.com        package registry
license.unity3d.com          licensing
activation.unity3d.com       licensing
core.cloud.unity3d.com       licensing
api.unity.com                licensing
login.unity.com              licensing
```

The download and package hosts are **verified** (packages resolved and the project
compiled). The licensing hosts are **unverified** until a real activation; add whatever a
refused request names.

In a session, the editor goes to `/opt/unity/6000.2.15f1/Editor/Unity` with the WebGL
module, about 10 GB, plus these packages:

```
xvfb xauth libgtk-3-0t64 libnss3 libgbm1 libasound2t64 libxss1 libxtst6 libxrandr2
libglu1-mesa libgl1 libegl1 libvulkan1 mesa-vulkan-drivers libglib2.0-0t64 libsecret-1-0
```

Without a licence it still resolves packages and compiles every assembly, which makes it
a useful compile check. Running tests needs activation.

Add `OP_SERVICE_ACCOUNT_TOKEN` as an environment variable in the environment's settings.
Never paste it into a chat. Variables reach a session's machine when it starts, not
mid-session.

### GitHub Actions

`tests.yml` and the deploy workflow (PR #92) both skip with a notice until their secrets
exist. Today they read the Unity login from Actions secrets directly:

- `UNITY_EMAIL`, `UNITY_PASSWORD`: the automation account
- `OP_SERVICE_ACCOUNT_TOKEN`: deploy only, for the itch key

See gap 3 for moving the Unity login behind 1Password as well.

## Remaining gaps

1. **The accounts don't exist yet.** The Deploy vault, the service account, the
   automation Unity ID and the Unity CI item are all still to create (sections 1 and 2).
2. **The cloud environment can't reach 1Password.** It needs the `op` CLI installed and
   1Password's download and API hosts allowed. The API host depends on the account's
   sign-in address, so it gets added once that's known.
3. **CI reads the Unity login twice over.** The workflows take `UNITY_EMAIL` and
   `UNITY_PASSWORD` as their own secrets, while machines read them from 1Password. Either
   accept the duplicate (simple, two places to rotate), or add a
   `1password/load-secrets-action` step before the Unity steps, as deploy already does for
   the itch key, so the only Actions secret is the token.
4. **Nothing reinstalls Unity in a new cloud session.** Each new machine starts without
   the editor. Wants a setup script on the environment, saved once activation and the test
   run have worked, so it only contains steps known to work. It costs a several minute
   download per new machine.
5. **`scripts/check.sh` assumes the Mac.** It defaults to the Mac editor path, finds the
   compiler tools by the Mac app layout, and runs PlayMode without a display. On Linux it
   needs the Linux path, the tools under `Editor/Data`, and PlayMode wrapped in
   `xvfb-run`. `build-web.sh` and `build-mac.sh` have the same default path.
6. **The deploy workflow (PR #92) has never run.** Unmerged, and untested against
   game-ci's builder with a custom build method.
7. **The itch project isn't created or published** (#74). Account steps, then the first
   `scripts/deploy-itch.sh`.
8. **Changing the company name moved local saves.** Unity keys PlayerPrefs by company and
   product name, so a Mac that played before the rename starts from an empty save.
   Expected, and better now than after players have saves.
