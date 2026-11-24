// Copies inside the keydown itself, the only moment every browser allows it.
mergeInto(LibraryManager.library, {
  ShareClipboardArm: function (text) {
    var state = Module.shareClipboard;
    if (!state) {
      state = Module.shareClipboard = { text: null, copied: 0 };

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

      document.addEventListener("keydown", function (e) {
        if (state.text === null || e.repeat || e.ctrlKey || e.metaKey || e.altKey) { return; }
        if (e.key !== "c" && e.key !== "C") { return; }

        var value = state.text;
        if (navigator.clipboard && navigator.clipboard.writeText) {
          navigator.clipboard.writeText(value).then(
            function () { state.copied = 1; },
            function () { if (fallback(value)) { state.copied = 1; } });
        } else if (fallback(value)) {
          state.copied = 1;
        }
      }, true);
    }

    state.text = UTF8ToString(text);
    state.copied = 0;
  },

  ShareClipboardDisarm: function () {
    if (Module.shareClipboard) {
      Module.shareClipboard.text = null;
      Module.shareClipboard.copied = 0;
    }
  },

  ShareClipboardTakeCopied: function () {
    var state = Module.shareClipboard;
    if (!state || !state.copied) { return 0; }
    state.copied = 0;
    return 1;
  },
});
