// The page's sign that the game booted, which a check can wait on instead of a loading bar.
mergeInto(LibraryManager.library, {
  PageReadyMark: function () {
    document.documentElement.setAttribute("data-ready", "1");
  },
});
