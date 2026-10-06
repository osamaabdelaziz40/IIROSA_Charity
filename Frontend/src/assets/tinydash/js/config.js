"use strict";
var base = {
        defaultFontFamily: "Overpass, sans-serif",
        primaryColor: "#1b68ff",
        secondaryColor: "#4f4f4f",
        successColor: "#3ad29f",
        warningColor: "#ffc107",
        infoColor: "#17a2b8",
        dangerColor: "#dc3545",
        darkColor: "#343a40",
        lightColor: "#f2f3f6",
    },
    extend = {
        primaryColorLight: tinycolor(base.primaryColor).lighten(10).toString(),
        primaryColorLighter: tinycolor(base.primaryColor).lighten(30).toString(),
        primaryColorDark: tinycolor(base.primaryColor).darken(10).toString(),
        primaryColorDarker: tinycolor(base.primaryColor).darken(30).toString(),
    },
    chartColors = [base.primaryColor, base.successColor, "#6f42c1", extend.primaryColorLighter],
    colors = { bodyColor: "#6c757d", headingColor: "#495057", borderColor: "#e9ecef", backgroundColor: "#f8f9fa", mutedColor: "#adb5bd", chartTheme: "light" },
    darkColor = { bodyColor: "#adb5bd", headingColor: "#e9ecef", borderColor: "#212529", backgroundColor: "#495057", mutedColor: "#adb5bd", chartTheme: "dark" },
    curentTheme = localStorage.getItem("mode"),
    dark = document.querySelector("#darkTheme"),
    light = document.querySelector("#lightTheme"),
    navy = document.querySelector("#navyTheme"),
    switcher = document.querySelector("#modeSwitcher");

// Themes are separate TinyDash compiles, each on its own <link> in index.html
// (#lightTheme / #darkTheme / #navyTheme). Exactly one stays enabled.
var themeLinks = { light: light, dark: dark, navy: navy };

function applyTheme(name) {
    if (themeLinks[name] === undefined || themeLinks[name] === null) {
        name = "light";
    }
    for (var key in themeLinks) {
        if (themeLinks[key]) {
            themeLinks[key].disabled = key !== name;
        }
    }
    if (switcher) {
        switcher.dataset.mode = name;
    }
}

// Legacy light/dark toggle kept for the #modeSwitcher click binding in apps.js.
// The header theme dropdown (main-layout) writes the mode directly instead.
function modeSwitch() {
    if (dark === undefined || dark === null || light === undefined || light === null) {
        dark = document.querySelector("#darkTheme");
        light = document.querySelector("#lightTheme");
    }
    var o = localStorage.getItem("mode");
    applyTheme(o === "dark" ? "light" : "dark");
    localStorage.setItem("mode", o === "dark" ? "light" : "dark");
}

// Boot: apply the saved theme, falling back to the body.dark class the shell
// ships with, exactly like the original two-theme logic did.
curentTheme = localStorage.getItem("mode");
if (curentTheme !== "light" && curentTheme !== "dark" && curentTheme !== "navy") {
    curentTheme = typeof $ !== "undefined" && $("body").hasClass("dark") ? "dark" : "light";
    localStorage.setItem("mode", curentTheme);
}
applyTheme(curentTheme);
if (curentTheme === "dark") {
    colors = darkColor;
}
