// Starts the story and each place's daily headless; check-web.sh and store-images.sh run it.
// Usage: node scripts/web-run.js <build> <captures> [--size=WxH] [--only=<run>]; exits 0, 1 or 2.
const { execSync } = require("child_process");
const fs = require("fs");
const http = require("http");
const path = require("path");

// A global install isn't on Node's module path, so look there too.
function loadPlaywright() {
  try {
    return require("playwright");
  } catch (e) {
    const globalRoot = execSync("npm root -g", { encoding: "utf8" }).trim();
    return require(path.join(globalRoot, "playwright"));
  }
}

let chromium;
try {
  ({ chromium } = loadPlaywright());
} catch (e) {
  console.error("web-run: no Playwright: npm i -g playwright@1.63.0 && npx playwright@1.63.0 install chromium");
  process.exit(2);
}

const RUNS = ["story", "daily-office", "daily-school", "daily-house", "title-click", "no-webgl2", "itch-frame"];

function usage(problem) {
  console.error(`web-run: ${problem}\n  usage: node scripts/web-run.js <build> <captures> [--size=WxH] [--only=<run>]`);
  process.exit(2);
}

const positional = [];
let size = [960, 600];
let only = null;
for (const arg of process.argv.slice(2)) {
  if (arg.startsWith("--size=")) {
    const m = /^(\d+)x(\d+)$/.exec(arg.slice(7));
    if (!m || +m[1] === 0 || +m[2] === 0) {
      usage(`bad size ${arg}`);
    }
    size = [+m[1], +m[2]];
  } else if (arg.startsWith("--only=")) {
    only = arg.slice(7);
    if (!RUNS.includes(only)) {
      usage(`no run called ${only}; there are ${RUNS.join(", ")}`);
    }
  } else if (arg.startsWith("--")) {
    usage(`unknown option ${arg}`);
  } else {
    positional.push(arg);
  }
}
const [root, out] = positional;
if (positional.length !== 2 || !fs.existsSync(path.join(root, "index.html"))) {
  usage("needs a build folder with index.html and a captures folder");
}
fs.mkdirSync(out, { recursive: true });

// Places.Story's order. A change there shows up as the wrong data-place, never a quiet pass.
const PLACES = ["office", "school", "house"];

// DailySeed.For, ported: the place a date's daily wears.
function dailyPlace(date) {
  const days = Math.round((Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate()) - Date.UTC(2020, 0, 1)) / 86400000);
  let h = Math.imul(days, 2654435761) >>> 0;
  h = (h ^ (h >>> 15)) >>> 0;
  h = Math.imul(h, 0x85ebca6b) >>> 0;
  h = (h ^ (h >>> 13)) >>> 0;
  return PLACES[(h & 0x7fffffff) % PLACES.length];
}

function dayFor(place) {
  for (let i = 0; i < 400; i++) {
    const d = new Date(Date.now() + i * 86400000);
    if (dailyPlace(d) === place) {
      return new Date(Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate(), 12));
    }
  }
  throw new Error("no day in the next 400 wears " + place);
}

const TYPES = { ".html": "text/html", ".js": "text/javascript", ".wasm": "application/wasm", ".json": "application/json", ".css": "text/css", ".png": "image/png", ".ico": "image/x-icon" };

function serve(dir) {
  return new Promise((resolve) => {
    const base = path.resolve(dir);
    const server = http.createServer((req, res) => {
      // Stands in for itch's page: the game in an iframe from another origin, behind a button.
      if (req.url.startsWith("/__parent.html")) {
        const frame = new URL(req.url, "http://x").searchParams.get("frame");
        res.writeHead(200, { "Content-Type": "text/html" });
        res.end(`<!DOCTYPE html><html><body style="margin:0;height:4000px;background:#222">
<button id="launch" style="margin:40px;font-size:20px">Run game</button>
<div id="slot"></div>
<script>
document.querySelector("#launch").addEventListener("click", function () {
  var f = document.createElement("iframe");
  f.src = ${JSON.stringify(frame)};
  f.width = 960; f.height = 600;
  f.setAttribute("allow", "autoplay; fullscreen *; gamepad");
  f.setAttribute("allowfullscreen", "");
  f.style.border = "0";
  document.querySelector("#slot").appendChild(f);
  this.remove();
});
</script></body></html>`);
        return;
      }

      // No favicon is fine; a 404 for it would show as a console error and fail every run.
      if (req.url === "/favicon.ico") {
        res.writeHead(204);
        res.end();
        return;
      }

      let name;
      try {
        name = decodeURIComponent(req.url.split("?")[0]).replace(/^\/+/, "");
      } catch (e) {
        res.writeHead(400);
        res.end();
        return;
      }

      const file = path.resolve(base, name || "index.html");
      if (!file.startsWith(base + path.sep) || !fs.existsSync(file) || fs.statSync(file).isDirectory()) {
        res.writeHead(404);
        res.end();
        return;
      }
      res.writeHead(200, { "Content-Type": TYPES[path.extname(file)] || "application/octet-stream" });
      fs.createReadStream(file).pipe(res);
    });
    server.listen(0, "127.0.0.1", () => resolve(server));
  });
}

async function scenario(browser, url, name, { key, day, expect, click }) {
  const context = await browser.newContext({ viewport: { width: size[0], height: size[1] } });
  const page = await context.newPage();
  const errors = [];
  page.on("console", (m) => {
    if (m.type() === "error") {
      errors.push(m.text().slice(0, 200));
    }
  });
  page.on("pageerror", (e) => errors.push(String(e).slice(0, 200)));

  // Shift the clock rather than freeze it: Unity's loader needs time to pass.
  if (day) {
    await page.addInitScript((target) => {
      const Real = Date;
      const offset = target - Real.now();
      class Shifted extends Real {
        constructor(...a) { if (a.length === 0) { super(Real.now() + offset); } else { super(...a); } }
        static now() { return Real.now() + offset; }
      }
      globalThis.Date = Shifted;
    }, day.getTime());
  }

  const failures = [];
  try {
    await page.goto(url);
    await page.waitForFunction(() => document.documentElement.getAttribute("data-ready") === "1", null, { timeout: 150000 });
    await page.waitForTimeout(1500);
    await page.screenshot({ path: path.join(out, name + "-title.png") });
    if (await page.evaluate(() => document.documentElement.getAttribute("data-run")) !== "title") {
      failures.push("the title didn't mark the page");
    }

    // The click a player makes to focus the game in a page must not start a run.
    if (click) {
      await page.click("canvas");
      // A run takes a fade and a scene load to mark the page, so wait out a slow one.
      const started = await page
        .waitForFunction(() => document.documentElement.getAttribute("data-runs") !== null, null, { timeout: 5000 })
        .then(() => true, () => false);
      if (started) {
        failures.push("a click on the title started a run");
      }
    }
    await page.focus("canvas");
    await page.keyboard.press(key);
    await page.waitForFunction(() => document.documentElement.getAttribute("data-runs") !== null, null, { timeout: 60000 });
    const run = await page.evaluate(() => ({
      mode: document.documentElement.getAttribute("data-run"),
      place: document.documentElement.getAttribute("data-place"),
    }));
    if (run.mode !== expect.mode || run.place !== expect.place) {
      failures.push(`started ${run.mode} in ${run.place}, expected ${expect.mode} in ${expect.place}`);
    }
    await page.waitForTimeout(6000);
    await page.screenshot({ path: path.join(out, name + "-run.png") });
  } catch (e) {
    failures.push(String(e.message || e).split("\n")[0]);
  }

  failures.push(...errors.map((e) => "console: " + e));
  await context.close();
  console.log(`${failures.length ? "FAIL" : "ok  "} ${name}${failures.length ? "\n  " + failures.join("\n  ") : ""}`);
  return failures.length === 0;
}

// A browser without WebGL 2 gets a sentence in place of the loading screen, not Unity's message.
async function noWebGL2(browser, url) {
  const context = await browser.newContext({ viewport: { width: size[0], height: size[1] } });
  const page = await context.newPage();
  const failures = [];
  const loaded = [];
  page.on("request", (r) => {
    if (r.url().includes("/Build/")) {
      loaded.push(r.url());
    }
  });
  await page.addInitScript(() => {
    const real = HTMLCanvasElement.prototype.getContext;
    HTMLCanvasElement.prototype.getContext = function (kind, ...rest) {
      return kind === "webgl2" ? null : real.call(this, kind, ...rest);
    };
  });
  try {
    await page.goto(url);
    await page.waitForTimeout(1500);
    await page.screenshot({ path: path.join(out, "no-webgl2.png") });
    const shown = await page.evaluate(() => ({
      error: getComputedStyle(document.querySelector("#error")).display !== "none",
      loading: getComputedStyle(document.querySelector("#loading")).display !== "none",
      text: document.querySelector("#error").textContent,
    }));
    if (!shown.error || !shown.text.startsWith("This browser can't run the game")) {
      failures.push("no sentence about WebGL 2: " + JSON.stringify(shown.text).slice(0, 120));
    }
    if (shown.loading) {
      failures.push("the loading screen is still up under the error");
    }
    if (loaded.length) {
      failures.push("Unity loaded anyway: " + loaded[0]);
    }
  } catch (e) {
    failures.push(String(e.message || e).split("\n")[0]);
  }
  await context.close();
  console.log(`${failures.length ? "FAIL" : "ok  "} no-webgl2${failures.length ? "\n  " + failures.join("\n  ") : ""}`);
  return failures.length === 0;
}

// As itch embeds it: the player clicks the page's launch button, never the game, then presses
// Space. The game must have the keys, and the page mustn't scroll from under it.
async function itchFrame(browser, parentUrl) {
  const context = await browser.newContext({ viewport: { width: size[0] + 100, height: size[1] + 200 } });
  const page = await context.newPage();
  const failures = [];
  const errors = [];
  page.on("console", (m) => {
    if (m.type() === "error") {
      errors.push(m.text().slice(0, 200));
    }
  });
  try {
    await page.goto(parentUrl);
    await page.click("#launch");
    const frame = await (await page.waitForSelector("iframe")).contentFrame();
    await frame.waitForFunction(() => document.documentElement.getAttribute("data-ready") === "1", null, { timeout: 150000 });
    await page.waitForTimeout(1500);
    await page.keyboard.press("Space");
    const started = await frame
      .waitForFunction(() => document.documentElement.getAttribute("data-runs") !== null, null, { timeout: 15000 })
      .then(() => true, () => false);
    await page.waitForTimeout(500);
    await page.screenshot({ path: path.join(out, "itch-frame.png") });
    if (!started) {
      failures.push("Space after the launch button didn't reach the game");
    }
    const scrolled = await page.evaluate(() => window.scrollY);
    if (scrolled !== 0) {
      failures.push(`the page scrolled ${scrolled}px under the game`);
    }
  } catch (e) {
    failures.push(String(e.message || e).split("\n")[0]);
  }
  failures.push(...errors.map((e) => "console: " + e));
  await context.close();
  console.log(`${failures.length ? "FAIL" : "ok  "} itch-frame${failures.length ? "\n  " + failures.join("\n  ") : ""}`);
  return failures.length === 0;
}

(async () => {
  const server = await serve(path.resolve(root));
  const url = `http://127.0.0.1:${server.address().port}/index.html`;

  // A browser that won't start is setup, not a game that failed.
  let browser;
  try {
    browser = await chromium.launch({ args: ["--use-gl=angle", "--use-angle=swiftshader", "--enable-unsafe-swiftshader", "--mute-audio"] });
  } catch (e) {
    console.error("web-run: Chromium won't start: npx playwright@1.63.0 install chromium\n  " + String(e.message || e).split("\n")[0]);
    server.close();
    process.exit(2);
  }

  let ok = true;
  if (!only || only === "story") {
    ok = await scenario(browser, url, "story", { key: "Space", expect: { mode: "story", place: "office" } });
  }
  for (const place of PLACES) {
    if (!only || only === "daily-" + place) {
      const day = dayFor(place);
      ok = (await scenario(browser, url, "daily-" + place, { key: "Enter", day, expect: { mode: "daily", place } })) && ok;
    }
  }

  if (!only || only === "itch-frame") {
    // localhost and 127.0.0.1 are different origins, as itch's page and its game host are.
    const port = server.address().port;
    const parent = `http://localhost:${port}/__parent.html?frame=` + encodeURIComponent(`http://127.0.0.1:${port}/index.html`);
    ok = (await itchFrame(browser, parent)) && ok;
  }
  if (!only || only === "no-webgl2") {
    ok = (await noWebGL2(browser, url)) && ok;
  }
  if (!only || only === "title-click") {
    const day = dayFor("office");
    ok = (await scenario(browser, url, "title-click", { key: "Enter", day, click: true, expect: { mode: "daily", place: "office" } })) && ok;
  }

  await browser.close();
  server.close();
  console.log(ok ? `web-run: every run started, captures in ${out}` : "web-run: failed");
  process.exit(ok ? 0 : 1);
})();
