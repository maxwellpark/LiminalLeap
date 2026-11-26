// Drives a WebGL build in headless Chromium: starts the story and each place's daily, checks the
// page says the right run started, fails on any console error, and saves captures.
// Usage: node scripts/web-run.js <build folder> <captures folder>
// Exit codes: 0 passed, 1 failed, 2 setup (no Playwright, no build).
const fs = require("fs");
const http = require("http");
const path = require("path");

let chromium;
try {
  ({ chromium } = require("playwright"));
} catch (e) {
  console.error("web-run: Playwright isn't installed: npm i -g playwright && npx playwright install chromium");
  process.exit(2);
}

const [root, out] = process.argv.slice(2);
if (!root || !out || !fs.existsSync(path.join(root, "index.html"))) {
  console.error("web-run: usage: node scripts/web-run.js <build folder> <captures folder>");
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
    const server = http.createServer((req, res) => {
      const file = path.join(dir, decodeURIComponent(req.url.split("?")[0]).replace(/^\/+/, "") || "index.html");
      if (!file.startsWith(path.resolve(dir)) || !fs.existsSync(file) || fs.statSync(file).isDirectory()) {
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
  const context = await browser.newContext({ viewport: { width: 960, height: 600 } });
  const page = await context.newPage();
  const errors = [];
  page.on("console", (m) => { if (m.type() === "error") errors.push(m.text().slice(0, 200)); });
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
  const browser = await chromium.launch({ args: ["--use-gl=angle", "--use-angle=swiftshader", "--enable-unsafe-swiftshader", "--mute-audio"] });

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
