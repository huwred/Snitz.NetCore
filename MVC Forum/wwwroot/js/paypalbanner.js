    const banner = document.getElementById("donationBanner");
    const dismissedAt = localStorage.getItem("donationBannerDismissedAt");

    // Check if banner should be hidden or shown
    if (dismissedAt) {
        const now = new Date().getTime();
        const fourteenDays = 14 * 24 * 60 * 60 * 1000; // 14 days in ms
        if (now - parseInt(dismissedAt, 10) < fourteenDays) {
            banner.style.display = "none"; // still within 14 days
        }
    }

    function closeBanner() {
        banner.style.display = "none";
        localStorage.setItem("donationBannerDismissedAt", new Date().getTime());
    }
