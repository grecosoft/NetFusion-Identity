namespace NetFusion.Identity.Client.Models.Dashboard;

public class DashboardModel
{
    public string Title { get; }
    public ApplicationModel[] Applications { get; } = Array.Empty<ApplicationModel>();

    public DashboardModel(string title)
    {
        Title = title;
    }
    public DashboardModel(string title, ApplicationModel[] applications)
    {
        Title = title;
        Applications = applications;
    }

    public bool HasApplications => Applications.Any();
}