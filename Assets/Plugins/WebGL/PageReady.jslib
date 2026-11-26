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
    root.setAttribute("data-laps", "0");
  },

  // Not counted as a run: data-runs stays as it was.
  PageTitleMark: function (dailyRuns) {
    var root = document.documentElement;
    root.setAttribute("data-run", "title");
    root.setAttribute("data-place", "none");
    root.setAttribute("data-daily-runs", String(dailyRuns));
  },

  // Unity's web audio waits for a gesture in this frame; a sound queued before that starts as
  // far in as the time already gone, so a short one is never heard.
  PageAudioRunning__deps: ["$WEBAudio"],
  PageAudioRunning: function () {
    var running = WEBAudio.audioContext !== null && WEBAudio.audioContext.state === "running";
    var mark = running ? "running" : "waiting";
    var root = document.documentElement;
    if (root.getAttribute("data-audio") !== mark) {
      root.setAttribute("data-audio", mark);
    }
    return running ? 1 : 0;
  },

  PageMarkSting: function () {
    document.documentElement.setAttribute("data-sting", "1");
  },

  PageLapMark: function () {
    var root = document.documentElement;
    root.setAttribute("data-laps", String((parseInt(root.getAttribute("data-laps") || "0", 10)) + 1));
  },
});
