﻿function SetupThemeToggle() // Source: https://dev.to/ananyaneogi/create-a-dark-light-mode-switch-with-css-variables-34l8
{
    var currentTheme = "dark";
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
                currentTheme = value;
                toggleSwitch.checked = false;
            }
        },
    })
    .done(function()
    {
        document.documentElement.setAttribute('data-theme', currentTheme);
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
            data: { key: "Theme", value: theme },
            success: function(data)
            {
                //if(data)
                //{
                //    console.log("YES");
                //}

                //else
                //{
                //    console.log("NO");
                //}
            },
        });
    }
}

function SetupPinToggle()
{
    const navbarPinBtn = document.getElementById("pin-nav-btn");
    const navbar = document.querySelector('.navbar');
    const body = document.querySelector('.content-body');
    var isPinned = false;

    $.ajax
    ({
        url: "/Base/GetSessionString",
        data: { key: "Pinned" },
        success: function(value)
        {
            if(value != null && value == "true")
            {
                isPinned = true;
            }
        },
    })
    .done(function()
    {
        navbarPinBtn.onclick = function()
        {
            isPinned = !isPinned;
            updatePin();
        }

        updatePin();
    });

    function updatePin()
    {
        if(isPinned)
        {
            navbar.classList.add("fixed-top");
            navbarPinBtn.textContent = "Unpin";

            if(IsMobile())
            {
                body.setAttribute("style", "padding-top: 110px;");
            }

            else
            {
                body.setAttribute("style", "padding-top: 80px;");
            }
        }

        else
        {
            navbar.classList.remove("fixed-top");
            body.setAttribute("style", "padding-top: none;");
            navbarPinBtn.textContent = "Pin";
        }

        $.ajax
        ({
            url: "/Base/SetSessionString",
            data: {key: "Pinned", value: isPinned.toString()},
            success: function(data)
            {
                //if(data)
                //{
                //    console.log("YES");
                //}

                //else
                //{
                //    console.log("NO");
                //}
            },
        });
    }
}

function SetupMobileChanges()
{
    var isMobile = IsMobile();

    function updateMobileStatus()
    {
        if(isMobile != IsMobile())
        {
            isMobile = !isMobile;

            if(isMobile)
            {
                document.documentElement.setAttribute("data-device", "mobile");
            }

            else
            {
                document.documentElement.setAttribute("data-device", "desktop");
            }
        }
    }

    window.onresize = function()
    {
        updateMobileStatus();
    }

    updateMobileStatus();
}

function IsMobile() // Source: https://stackoverflow.com/a/11381730
{
    let check = false;
    (function(a) {if(/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true;})(navigator.userAgent || navigator.vendor || window.opera);
    return check;
}