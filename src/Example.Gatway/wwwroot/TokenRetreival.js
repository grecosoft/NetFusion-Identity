alert("TEST");

fetch('https://localhost:7006/api/token/jwt', { credentials: "include" })
    .then(response => console.log(response.text()));
