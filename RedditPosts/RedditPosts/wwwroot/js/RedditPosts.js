// Infinite Scroll Source: https://stackoverflow.com/a/64730174
// To the Top Button: https://www.w3schools.com/howto/howto_js_scroll_to_top.asp

function Scrolling(iTable, iAction, iParams) 
{
    this.table = iTable;        // Reference to the table where data should be added
    this.action = iAction;      // Name of the conrtoller action
    this.params = iParams;      // Additional parameters to pass to the controller
    this.loading = false;       // true if asynchronous loading is in process

    this.AddTableLines = function(firstItem) 
    {
        this.loading = true;
        this.params.firstItem = firstItem;
        // $("#footer").css("display", "block"); // show loading info
        $.ajax({
            type: 'POST',
            url: self.action,
            data: self.params,
            dataType: "html"
        })
        .done(function(result) 
        {
            if(result) 
            {
                $("#" + self.table).append(result);
                self.loading = false;
            }
        })
        .fail(function(xhr, ajaxOptions, thrownError) 
        {
            console.log("Error in AddTableLines:", thrownError);
        })
        .always(function() 
        {
            // $("#footer").css("display", "none"); // hide loading info
        });
    }

    // To the Top Button: https://www.w3schools.com/howto/howto_js_scroll_to_top.asp
    var topScrollBtn = document.getElementById("topScrollBtn");

    var self = this;
    window.onscroll = function() 
    {
        onScrollTopBtnShow();
        onScrollInfinite();
    };

    function onScrollInfinite() 
    {
        if((window.innerHeight + window.scrollY) >= document.body.offsetHeight) 
        {
            //User is currently at the bottom of the page
            if(!self.loading) 
            {
                var itemCount = $('#' + self.table + ' tr').length - 1;
                self.AddTableLines(itemCount);
            }
        }
    }

    // When the user scrolls down 20px from the top of the document, show the button
    function onScrollTopBtnShow() 
    {
        if(document.body.scrollTop > 20 || document.documentElement.scrollTop > 20)
        {

            topScrollBtn.style.display = "block";
        }

        else
        {
            topScrollBtn.style.display = "none";
        }
    }

    this.AddTableLines(0);
}

function SetSelect(jsonObj) 
{
    var types = [];
    for(var i in jsonObj) // Source: https://stackoverflow.com/a/14528472
    {
        types.push(jsonObj[i]);
    }

    var deselectBtn = document.getElementById("deselectTypesBtn");
    var selectBtn = document.getElementById("selectTypesBtn");

    function setSelectCheckbox(isSelected, item, index) 
    {
        var checkbox = document.getElementById(item);
        checkbox.checked = isSelected;
    }

    selectBtn.onclick = function() 
    {
        types.forEach(function(item, index) 
        {
            setSelectCheckbox(true, item, index)
        });
    }

    deselectBtn.onclick = function() 
    {
        types.forEach(function(item, index) 
        {
            setSelectCheckbox(false, item, index)
        });
    }
}
