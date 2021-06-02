// Infinite Scroll Source: https://stackoverflow.com/a/64730174
// To the Top Button: https://www.w3schools.com/howto/howto_js_scroll_to_top.asp

function SetupInfiniteScrolling(iTable, iAction, iParams) 
{
    this.table = iTable;        // Reference to the table where data should be added
    this.action = iAction;      // Name of the conrtoller action
    this.params = iParams;      // Additional parameters to pass to the controller
    this.loading = false;       // true if asynchronous loading is in process
    var topScrollBtn = document.getElementById("topScrollBtn");
    var loadingPostsDisplay = document.getElementById("loadingPosts");

    this.AddTableLines = function(lastRowNumber) 
    {
        this.loading = true;
        this.params.lastRowNumber = lastRowNumber;
        loadingPostsDisplay.textContent = "LOADING POSTS"; // show loading info

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
            loadingPostsDisplay.textContent = ""; // hide loading info
            ShowAutoScrollBtn();
        });
    }

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
                var tableElement = document.getElementById(self.table);
                var lastRowNumber = tableElement.rows[tableElement.rows.length - 1].id;

                self.AddTableLines(lastRowNumber);
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

    topScrollBtn.onclick = function()
    {
        document.body.scrollTop = 0;
        document.documentElement.scrollTop = 0;
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

function SetupAutoScroll() // Source: https://stackoverflow.com/a/9837823
{
    var autoScrollBtn = document.getElementById("autoScrollBtn");
    var isAutoScrolling = false;

    autoScrollBtn.onclick = function()
    {
        isAutoScrolling = !isAutoScrolling;
        AutoScroll();
    }

    function AutoScroll()
    {
        if(isAutoScrolling)
        {
            window.scrollBy(0, 3);
            autoScrollBtn.textContent = "Stop Scroll";
            scrolldelay = setTimeout(AutoScroll, 10);
        }

        else
        {
            autoScrollBtn.textContent = "Auto Scroll";
        }
    }

    $(document).click(function(event) // Source: https://stackoverflow.com/a/3028037
    {
        var $target = $(event.target);
        if(!$target.closest('#autoScrollBtn').length && isAutoScrolling)
        {
            autoScrollBtn.click();
        }
    });
}

function HidePostToggle(id)
{
    var hideBtn = document.getElementById("hidePostBtn_" + id);

    $.ajax
    ({
        url: "/RedditPosts/Hide",
        data: {id},
        success: function(data, textStatus, jqXHR) // Data is the return value
        {
            if(data)
            {
                hideBtn.innerHTML = "Show";
            }

            else
            {
                hideBtn.innerHTML = "Hide";
            }

            $('#' + 'HideModal_' + id).modal('hide');
        },
    });
}

function SetupRandomSeedDisplay()
{
    var sortingDropdown = document.getElementById("SortingSetting");
    var randomSeedDisplay = document.getElementById("randomSeedDisplay");

    sortingDropdown.onchange = function()
    {
        updateSortDisplay();
    }

    function updateSortDisplay()
    {
        randomSeedDisplay.style.display = sortingDropdown.value == "Random" ? "inline-block" : "none";
    }

    updateSortDisplay();
}

function FavoritePostToggle(id)
{
    $.ajax
    ({
        url: "/RedditPosts/Favorite",
        data: {id}
    });
}

function SetupDateRangeDisplay()
{
    var useDateRangeCheckbox = document.getElementById("UseDateRange");
    var dateRangesDisplay = document.getElementById("dateRanges");

    useDateRangeCheckbox.onchange = function()
    {
        updatedateRangesDisplay();
    }

    function updatedateRangesDisplay()
    {
        dateRangesDisplay.style.display = useDateRangeCheckbox.checked ? "inline-block" : "none";
    }

    updatedateRangesDisplay();
}

function ImageOnClick(id)
{
    var imgIdToFind = "";

    if(id.includes("-right"))
    {
        imgIdToFind = id.replace("-right", "");
        console.log("right");
    }

    else if(id.includes("-left"))
    {
        imgIdToFind = id.replace("-left", "");
        console.log("left");
    }

    else
    {
        return;
    }

    var idToFind = '#' + imgIdToFind;
}

function SetupMp4VideoAutoPlay()
{
    function isScrolledIntoView(element)
    {
        var elementTop = element.getBoundingClientRect().top,
            elementBottom = element.getBoundingClientRect().bottom;

        return elementTop >= 0 && elementBottom <= window.innerHeight;
    }

    window.addEventListener("scroll", function()
    {
        $(".mp4-video").each(function()
        {
            var video = this;
            var isInView = isScrolledIntoView(video);
            var isVidPlaying = isPlaying(video);

            if(!isVidPlaying && isInView)
            {
                if(!("ManualPause" in video && video.ManualPause))
                {
                    try
                    {
                        video.play();
                    }

                    catch(error)
                    {

                    }
                }
                
            }

            else if(isVidPlaying && !isInView)
            {
                video.ProgramaticallyPaused = true;

                try
                {
                    video.pause();
                }

                catch(error)
                {

                }

                setTimeout(function()
                {
                    video.ProgramaticallyPaused = false;
                }, 100);
            }
        });
    })

    function isPlaying(video)
    {
        // Source: https://stackoverflow.com/a/6877530
        return video.currentTime > 0 && !video.paused && !video.ended && video.readyState > 2;
    }
}

function VideoPause(videoElement)
{
    if(!("ProgramaticallyPaused" in videoElement && videoElement.ProgramaticallyPaused))
    {
        videoElement.ManualPause = true;
    }
}

function AutoplayCarousel(carouselId)
{
    var btnTxt = $('#' + carouselId + '-autoplay-btn').text();

    if(btnTxt.includes("Start"))
    {
        $('#' + carouselId).carousel('cycle');
        $('#' + carouselId + '-autoplay-btn').text('Stop Autoplay');
    }

    else
    {
        $('#' + carouselId).carousel('pause');
        $('#' + carouselId + '-autoplay-btn').text('Start Autoplay');
    }
}

function ShowAutoScrollBtn()
{
    var autoScrollBtn = document.getElementById("autoScrollBtn");

    if(document.documentElement.scrollHeight > document.documentElement.clientHeight)
    {

        autoScrollBtn.style.display = "block";
    }

    else
    {
        autoScrollBtn.style.display = "none";
    }
}