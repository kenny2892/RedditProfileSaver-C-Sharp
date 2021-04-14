// Source: https://dev.to/ananyaneogi/create-a-dark-light-mode-switch-with-css-variables-34l8

function SetupThemeToggle()
{
    const currentTheme = localStorage.getItem('theme') ? localStorage.getItem('theme') : null;
    const toggleSwitch = document.querySelector('.theme-switch input[type="checkbox"]');
    toggleSwitch.checked = true;

    if(currentTheme)
    {
        document.documentElement.setAttribute('data-theme', currentTheme);

        if(currentTheme === 'light')
        {
            toggleSwitch.checked = false;
        }
    }

    function switchTheme(e)
    {
        if(e.target.checked)
        {
            document.documentElement.setAttribute('data-theme', 'dark');
            localStorage.setItem('theme', 'dark');
        }

        else
        {
            document.documentElement.setAttribute('data-theme', 'light');
            localStorage.setItem('theme', 'light');
        }
    }

    toggleSwitch.addEventListener('change', switchTheme, false);
}