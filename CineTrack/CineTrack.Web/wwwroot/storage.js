window.cineStorage = {
  getUser: function () {
    return localStorage.getItem("cinetrack-user");
  },
  setUser: function (name) {
    localStorage.setItem("cinetrack-user", name);
  },
  signOut: function () {
    localStorage.removeItem("cinetrack-user");
  },
  key: function () {
    var user = localStorage.getItem("cinetrack-user") || "";
    return "cinetrack-v1-" + user.toLowerCase();
  },
  load: function () {
    return localStorage.getItem(window.cineStorage.key());
  },
  save: function (value) {
    localStorage.setItem(window.cineStorage.key(), value);
  }
};