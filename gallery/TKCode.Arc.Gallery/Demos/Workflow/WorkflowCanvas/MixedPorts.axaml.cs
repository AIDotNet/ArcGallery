using Avalonia;
using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>An expense approval flow: a trigger above a row of steps whose ports face left and right, and a last step below the row.</summary>
public partial class WorkflowCanvasMixedPortsDemo : UserControl
{
    public WorkflowCanvasMixedPortsDemo()
    {
        InitializeComponent();
        var graph = new WorkflowGraph();
        // The trigger keeps the canvas's top-to-bottom ports; the row below runs left to right.
        var submitted = Add(graph, new WorkflowNode("submitted", "Expense submitted")
        {
            Description = "Expense form", HasDefaultInput = false, Icon = ArcIcons.Inbox, State = WorkflowNodeState.Done,
            Position = new Point(40, 20)
        });
        var receipt = Add(graph, new WorkflowNode("receipt", "Check the receipt")
        {
            Description = "Amount and date", Result = "Matches the form", Duration = TimeSpan.FromMilliseconds(420),
            Icon = ArcIcons.Search, State = WorkflowNodeState.Done, PortLayout = WorkflowLayoutDirection.LeftToRight,
            Position = new Point(400, 180)
        });
        var manager = Add(graph, new WorkflowNode("manager", "Manager approval")
        {
            Description = "Team lead", Result = "Approved by Dana", Duration = TimeSpan.FromMinutes(12),
            Icon = ArcIcons.Users, State = WorkflowNodeState.Done, PortLayout = WorkflowLayoutDirection.LeftToRight,
            Position = new Point(740, 180)
        });
        var finance = Add(graph, new WorkflowNode("finance", "Finance review")
        {
            Description = "Budget code", Icon = ArcIcons.Sparkles, State = WorkflowNodeState.Running,
            PortLayout = WorkflowLayoutDirection.LeftToRight, Position = new Point(1080, 180)
        });
        // Back to the canvas's own ports: an edge from a left-to-right step into a top-to-bottom one.
        var pay = Add(graph, new WorkflowNode("pay", "Pay out")
        {
            Description = "Next payroll run", Icon = ArcIcons.Zap, Position = new Point(1280, 360)
        });
        graph.Connect(submitted, receipt);
        graph.Connect(receipt, manager);
        graph.Connect(manager, finance);
        graph.Connect(finance, pay);
        Canvas.Graph = graph;
    }

    private static WorkflowNode Add(WorkflowGraph graph, WorkflowNode node)
    {
        graph.Nodes.Add(node);
        return node;
    }
}
