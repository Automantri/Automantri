using Automantri.Application.Frontend;

namespace Automantri.Application.Frontend;

public interface IContentService
{
    IReadOnlyCollection<TestimonialDto> GetTestimonials();
    IReadOnlyCollection<ActivityItemDto> GetRecentActivity();
    PlatformStatsDto GetPlatformStats();
    JourneySectionDto GetJourney();
}

public sealed class ContentService : IContentService
{
    public IReadOnlyCollection<TestimonialDto> GetTestimonials() =>
    [
        new("1", "Priya Sharma", "Mumbai", "Hyundai Creta", "Automantri helped me save ₹45,000 on my Creta purchase.", 5, "https://randomuser.me/api/portraits/women/44.jpg", "₹45,000"),
        new("2", "Rajesh Kumar", "Bangalore", "Tata Nexon EV", "The TCO calculator made my EV decision crystal clear.", 5, "https://randomuser.me/api/portraits/men/32.jpg", "₹1.2L/year"),
        new("3", "Amit Mehta", "Delhi", "Maruti Grand Vitara", "Compared 5 SUVs in minutes. Best decision tool ever.", 4.8, "https://randomuser.me/api/portraits/men/75.jpg", "₹38,000"),
        new("4", "Sneha Reddy", "Hyderabad", "Kia Seltos", "Owner reviews gave me confidence before buying.", 4.9, "https://randomuser.me/api/portraits/women/65.jpg", "₹22,000")
    ];

    public IReadOnlyCollection<ActivityItemDto> GetRecentActivity() =>
    [
        new("1", "Rahul K.", "compared", "Creta vs Seltos", "2 min ago", "Mumbai"),
        new("2", "Anita S.", "booked test drive for", "Nexon EV", "5 min ago", "Bangalore"),
        new("3", "Vikram P.", "saved ₹32,000 on", "Grand Vitara", "8 min ago", "Pune"),
        new("4", "Meera D.", "viewed", "Sierra 2026", "12 min ago", "Chennai")
    ];

    public PlatformStatsDto GetPlatformStats() =>
        new(50_000, 4.9, "₹50 Cr", 125_000);

    public JourneySectionDto GetJourney() =>
        new(
            "Your Journey",
            "From first search",
            "to forever ownership",
            "Five stages. One platform. Every tool to own smarter.",
            "Ready to own smarter?",
            "Join 50,000+ car owners",
            "Get Started Free",
            "/find-cars",
            ["100% Unbiased", "50,000+ Users", "We Don't Sell Cars"],
            [
                new("discover", 1, "Discover", "Find & Compare",
                    "Vehicle Intelligence Score and TCO built into every search.",
                    "/find-cars", "search"),
                new("decide", 2, "Decide", "Sell or Keep?",
                    "Clear verdict based on resale value, costs and market demand.",
                    "/advice", "chart"),
                new("own", 3, "Own", "Track & Maintain",
                    "Service reminders, repair logs, and parts tracking.",
                    "/services", "wrench"),
                new("protect", 4, "Protect", "Insure Smart",
                    "Compare policies and get claims assistance.",
                    "/insure", "shield"),
                new("upgrade", 5, "Upgrade", "Scale Up",
                    "Know when to upgrade or manage your fleet.",
                    "/fleet", "trend")
            ]);
}
