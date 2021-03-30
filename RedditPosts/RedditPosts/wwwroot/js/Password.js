function PasswordSetup()
{
    // Source for Password Focus: https://stackoverflow.com/a/210764
    var password = document.getElementById("password");
    password.focus();

    // Source for Toggle: https://www.w3schools.com/howto/howto_js_toggle_password.asp
    var passwordCheckbox = document.getElementById("showPassword");
    passwordCheckbox.onclick = function()
    {
        togglePassword();
    };

    function togglePassword()
    {
        if(password.type === "password")
        {
            password.type = "text";
        }

        else
        {
            password.type = "password";
        }
    }
}