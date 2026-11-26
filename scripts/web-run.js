// Starts the story and each place's daily in a WebGL build, headless. scripts/check-web.sh runs it.
// Usage: node scripts/web-run.js <build> <captures> [--size=WxH]; exits 0 passed, 1 failed, 2 setup.
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

const args = process.argv.slice(2);
const [root, out] = args.filter((a) => !a.startsWith("--"));
const size = (args.find((a) => a.startsWith("--size=")) || "--size=960x600").slice(7).split("x").map(Number);
if (!root || !out || !fs.existsSync(path.join(root, "index.html")) || !(size[0] > 0 && size[1] > 0)) {
  console.error("web-run: usage: node scripts/web-run.js <build folder> <captures folder> [--size=WxH]");
  process.exit(2);
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

async function scenario(browser, url, name, { key, day, expect }) {
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

    // No click first: the title takes a click as any key, which starts the story.
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

  let ok = await scenario(browser, url, "story", { key: "Space", expect: { mode: "story", place: "office" } });
  for (const place of PLACES) {
    const day = dayFor(place);
    ok = (await scenario(browser, url, "daily-" + place, { key: "Enter", day, expect: { mode: "daily", place } })) && ok;
  }

  await browser.close();
  server.close();
  console.log(ok ? `web-run: every run started, captures in ${out}` : "web-run: failed");
  process.exit(ok ? 0 : 1);
})();
