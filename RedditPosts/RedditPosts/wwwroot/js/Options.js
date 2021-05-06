function SetupThemeToggle() // Source: https://dev.to/ananyaneogi/create-a-dark-light-mode-switch-with-css-variables-34l8
{
    const toggleSwitch = document.querySelector('.theme-switch input[type="checkbox"]');
    toggleSwitch.checked = true;

    $.ajax
    ({
        url: "/Base/GetSessionString",
        data: { key: "Theme" },
        success: function(value)
        {
            if(value != null && value == "light")
            {
                toggleSwitch.checked = false;
            }
        },
    })
    .done(function()
    {
        toggleSwitch.addEventListener('change', switchTheme, false);
    });

    function switchTheme(e)
    {
        var theme = "light";

        if(e.target.checked)
        {
            theme = "dark";
        }

        document.documentElement.setAttribute('data-theme', theme);

        $.ajax
        ({
            url: "/Base/SetSessionString",
            data: { key: "Theme", value: theme }
        });
    }
}