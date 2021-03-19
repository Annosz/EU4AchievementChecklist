// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
function showDetailedError() {
    var loggedInContainer = document.getElementById("loggedInContainer")
    if (loggedInContainer.classList.contains("clickable")) {
        document.getElementById("detailedErrorView").style.display = "contents";
        document.getElementById("showDetailedErrorText").style.display = "none";
        loggedInContainer.classList.remove("clickable");
    }
}