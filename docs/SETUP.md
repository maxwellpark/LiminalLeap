# Secrets, licensing and machine setup

How the credentials this project needs are stored and reached, on each machine that
needs them, and what is still not set up. Written 2026-11-24. Each step is marked
**verified** or **unverified**; tick things off here as they get done.

## The model

Every secret lives in 1Password, in the **LiminalLeap** vault, one item per service. The
repo holds only `op://` references to it, in `op/<purpose>.env` (`op/itch.env`,
`op/unity.env`), which are safe to commit. A machine reaches the vault one of two ways:

- **The Mac, interactively:** the 1Password desktop app with CLI integration, unlocked by
  Touch ID. Nothing stored.
- **Anywhere unattended** (a cloud session): a 1Password **service
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
| itch API key | `LiminalLeap / itch / api-key` | `scripts/deploy-itch.sh` |
| Unity login | `LiminalLeap / unity / username`, `password` | `scripts/unity-activate.sh` |
| Service account token | not in the vault | cloud environment variable |

## 1. The 1Password vault and service account

1. Create a vault named **LiminalLeap**. Keep only this project's automation credentials
   in it.
2. Add the itch key as the item **itch** (the comment in `op/itch.env` has the command).
3. Add the Unity login as the Login item **unity** (see `op/unity.env`). Use the
   dedicated account from section 2, not a personal one.
4. Create a service account with **read** access to **LiminalLeap** only, no other vaults.
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
- It needs no link to anyone's own Unity ID. The project isn't attached to a Unity
  organisation or cloud project (`organizationId` and `cloudProjectId` are empty in
  `ProjectSettings.asset`, and cloud services are off), so any account with a licence can
  open, test and build it. Don't add the account to an organisation unless the project
  starts using Unity's cloud services.
- Separate, but not throwaway: use an email address you control, such as an alias of your
  own, and keep the password in the **unity** item. You'll need to sign in to it on the web.
- Each new cloud container activates afresh, and a Personal licence can be active on only a
  few machines at once (check the current limit). End a session with
  `scripts/unity-activate.sh --return`; a container reclaimed before that leaves an
  activation behind, which you free from the account's licence page.
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
list for the second or so activation runs. The house rule is never to pass a secret as a
bare argument, and this is the one place that can't be met: there is no other way in.
Decided 2026-11-24: accepted on cloud machines only, with the automation account.

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

Off on this repo, which has no workflows. Tests, builds and deploys run on a machine: the Mac,
or a cloud session once Unity is activated there. No Actions secrets are needed. The old
workflows are in git history if Actions is ever turned back on.

## Decided, to build

- Decided 2026-11-24: cloud sessions get the `op` CLI from the LiminalLeap environment's
  setup script, with 1Password's hosts allowed. Waiting on the account's sign-in address.
- Decided 2026-11-24: compare these scripts with the shared libraries (unity-packages'
  `unity-verify`, shipwright) before deciding what moves there.
- Decided 2026-11-24: once activation works, the setup script installs Unity in every new
  session on the environment.

## Remaining gaps

1. **The accounts don't exist yet.** The LiminalLeap vault, its two items, the service
   account and the automation Unity ID are all still to create (sections 1 and 2).
2. **The cloud environment can't reach 1Password.** It needs the `op` CLI, installed only
   on the owner's word, and 1Password's download and API hosts allowed. The API host
   depends on the account's sign-in address, so it gets added once that's known.
3. **Nothing reinstalls Unity in a new cloud session.** Each new machine starts without
   the editor. Wants a setup script on the environment, saved once activation and the test
   run have worked, so it only contains steps known to work. It costs a several minute
   download per new machine.
4. ~~**`scripts/check.sh` assumes the Mac.**~~ Done: `scripts/unity-path.sh` holds the
   editor path, tool folder and launcher per OS, and `check.sh`, `build-web.sh` and
   `unity-activate.sh` read it. `check.sh --fast` is **verified** on Linux and catches a
   planted error. On a headless Linux box Unity runs under `xvfb-run`, which is
   **unverified** until there's a licence to run the suites with. A missing licence now
   says so instead of reporting no results.
5. **The itch project isn't created or published** (#74). Account steps, then the first
   `scripts/deploy-itch.sh`.
6. **Changing the company name moved local saves.** Unity keys PlayerPrefs by company and
   product name, so a Mac that played before the rename starts from an empty save.
   Expected, and better now than after players have saves.
