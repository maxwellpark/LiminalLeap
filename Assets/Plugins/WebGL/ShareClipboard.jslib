// Copies inside the click itself, the only moment every browser allows it.
mergeInto(LibraryManager.library, {
  ShareClipboardArm: function (text) {
    var state = Module.shareClipboard;
    if (!state) {
      state = Module.shareClipboard = { text: null, copied: 0, x: 0, y: 0, w: 0, h: 0, armed: 0 };

      // A hidden textarea, for browsers without the async clipboard or that refuse it.
      var fallback = function (value) {
        var area = document.createElement("textarea");
        area.value = value;
        area.setAttribute("readonly", "");
        area.style.position = "fixed";
        area.style.opacity = "0";
        document.body.appendChild(area);
        area.select();
        var ok = false;
        try { ok = document.execCommand("copy"); } catch (e) { ok = false; }
        document.body.removeChild(area);

        // Selecting took focus from the canvas, and the game hears no keys without it.
        if (Module.canvas) { Module.canvas.focus(); }
        return ok;
      };

      var copyAt = function (e) {
        var canvas = Module.canvas;
        if (state.text === null || !canvas || state.w <= 0) { return; }

        // Into Unity's screen pixels, which count up from the bottom left.
        var r = canvas.getBoundingClientRect();
        var x = (e.clientX - r.left) * canvas.width / r.width;
        var y = (r.bottom - e.clientY) * canvas.height / r.height;
        if (x < state.x || y < state.y || x > state.x + state.w || y > state.y + state.h) { return; }

        // Marked on the page too, so a browser check can tell the copy happened. A copy that
        // lands after the next summary armed belongs to the last one, so it's dropped.
        var armed = state.armed;
        var done = function () {
          if (armed !== state.armed) { return; }
          state.copied = 1;
          document.documentElement.setAttribute("data-copied", "1");
        };
        var value = state.text;
        if (navigator.clipboard && navigator.clipboard.writeText) {
          navigator.clipboard.writeText(value).then(done, function () { if (fallback(value)) { done(); } });
        } else if (fallback(value)) {
          done();
        }
      };

      // A press only grants the clipboard for a mouse; a finger or pen grants it on lifting.
      document.addEventListener("pointerdown", function (e) { if (e.pointerType === "mouse") { copyAt(e); } }, true);
      document.addEventListener("pointerup", function (e) { if (e.pointerType !== "mouse") { copyAt(e); } }, true);
    }

    state.text = UTF8ToString(text);
    state.copied = 0;
    state.w = 0;
    state.armed++;
    document.documentElement.removeAttribute("data-copied");
    document.documentElement.removeAttribute("data-copy-target");
  },

  ShareClipboardTarget: function (x, y, w, h) {
    var state = Module.shareClipboard;
    if (state) {
      state.x = x;
      state.y = y;
      state.w = w;
      state.h = h;
      // In Unity's screen pixels, from the bottom left, for a check to aim at.
      document.documentElement.setAttribute("data-copy-target", [x, y, w, h].join(","));
    }
  },

  ShareClipboardDisarm: function () {
    document.documentElement.removeAttribute("data-copy-target");
    document.documentElement.removeAttribute("data-copied");
    if (Module.shareClipboard) {
      Module.shareClipboard.text = null;
      Module.shareClipboard.copied = 0;
      Module.shareClipboard.w = 0;
    }
  },

  ShareClipboardTakeCopied: function () {
    var state = Module.shareClipboard;
    if (!state || !state.copied) { return 0; }
    state.copied = 0;
    return 1;
  },
});
