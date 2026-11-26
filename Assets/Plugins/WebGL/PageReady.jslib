// The page's sign that the game booted, which a check can wait on instead of a loading bar.
mergeInto(LibraryManager.library, {
  PageReadyMark: function () {
    document.documentElement.setAttribute("data-ready", "1");
  },

  // Counted too, so a check can wait for the next run rather than read the last one's.
  PageRunMark: function (mode, place) {
    var root = document.documentElement;
    root.setAttribute("data-run", UTF8ToString(mode));
    root.setAttribute("data-place", UTF8ToString(place));
    root.setAttribute("data-runs", String((parseInt(root.getAttribute("data-runs") || "0", 10)) + 1));
  },
});
