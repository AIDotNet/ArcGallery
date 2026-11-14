"""Generates a2ui/TKCode.Arc.A2UI.Chat/Showcases/ShowcaseLibrary.Rich.g.cs: the rich examples' A2UI messages.

The messages are written as Python data and serialized with json.dumps, so every line is valid JSON. All names, numbers
and text are made up for the demo. Run: python3 a2ui/tools/gen_rich_showcases.py
"""
import json, pathlib

B = "https://a2ui.org/specification/v1_0/catalogs/basic/catalog.json"
C = "https://github.com/AIDotNet/TKCode.Arc/a2ui/catalogs/arc/v1"  # the Arc catalog (A2UICatalogs.ArcCatalogId)

def env(kind, body): return json.dumps({"version": "1.0", kind: body}, ensure_ascii=False, separators=(",", ":"))
def create(sid, components, data=None, send=True):
    body = {"surfaceId": sid, "catalogId": B, "sendDataModel": send, "components": components}
    if data is not None: body["dataModel"] = data
    return env("createSurface", body)
def update(sid, components): return env("updateComponents", {"surfaceId": sid, "components": components})
def data(sid, path, value): return env("updateDataModel", {"surfaceId": sid, "path": path, "value": value})
def p(path): return {"@path": path}
def fmt(text): return {"@call": "formatString", "args": {"value": text}}
def c(id_, comp, **props):
    d = {"id": id_, "component": comp}
    for k, v in props.items():
        d[k.replace("__", "@")] = v
    return d
def x(id_, comp, **props):  # chat catalog component
    d = c(id_, comp, **props); d["@catalogId"] = C; return d
def text(id_, t, caption=False):
    return c(id_, "Text", text=t, **({"variant": "caption"} if caption else {}))
def button(id_, label, action, primary=False, checks=None, variant=None):
    b = c(id_, "Button", child=id_ + "_label", action=action)
    if primary: b["variant"] = "primary"
    if variant: b["variant"] = variant
    if checks: b["@checks"] = checks
    return [b, text(id_ + "_label", label)]
def event(name, **context): return {"event": {"name": name, "context": context}} if context else {"event": {"name": name}}
def required(path, message): return {"condition": {"@call": "required", "args": {"value": p(path)}}, "message": message}
def answer(prose, *lines): return prose + ("\n```a2ui\n" + "\n".join(lines) + "\n```\n" if lines else "\n")

MONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]

# ---------------------------------------------------------------- Sales dashboard
def sales_data(scale, region_name, shift):
    revenue = [round((82 + i * 6.5 + (i % 3) * 7 + shift * (i % 2)) * scale) for i in range(12)]
    target = [round((90 + i * 6) * scale) for i in range(12)]
    return {
        "regionName": region_name,
        "kpi": {"revenue": round(sum(revenue) / 1000, 2), "deals": round(318 * scale), "winRate": 27.4 + shift, "cycle": 41 - shift},
        "months": MONTHS,
        "series": [{"name": "Revenue", "values": revenue}, {"name": "Target", "values": target}],
        "channels": {"labels": ["Direct", "Partners", "Self-serve", "Marketplace"], "values": [round(sum(revenue) * f / 100) for f in (44 - shift, 26, 21 + shift, 9)]},
        "deals": [
            {"account": "Northwind Traders", "owner": "Mia Chen", "stage": "Negotiation", "amount": round(184000 * scale)},
            {"account": "Contoso Health", "owner": "Leo Park", "stage": "Proposal", "amount": round(142500 * scale)},
            {"account": "Fabrikam Logistics", "owner": "Ana Ruiz", "stage": "Discovery", "amount": round(96000 * scale)},
            {"account": "Litware Labs", "owner": "Sam Ito", "stage": "Negotiation", "amount": round(88200 * scale)},
            {"account": "Adatum Retail", "owner": "Mia Chen", "stage": "Proposal", "amount": round(61750 * scale)},
        ],
        "region": [],
    }

def sales_start():
    S = "sales_1"
    comps = [
        c("root", "Column", children=["header", "kpis", "split", "deals_card", "actions"]),
        c("header", "Row", align="center", justify="spaceBetween", children=["heading", "region"]),
        c("heading", "Column", weight=1, children=["title", "subtitle"]),
        text("title", "## Revenue review"),
        text("subtitle", fmt("FY 2026 · ${/regionName} · updated today"), caption=True),
        c("region", "ChoicePicker", variant="mutuallyExclusive", displayStyle="chips", options=[{"label": "Americas", "value": "americas"}, {"label": "EMEA", "value": "emea"}, {"label": "APAC", "value": "apac"}], value=p("/region")),
        c("kpis", "Row", children=["k1", "k2", "k3", "k4"]),
        x("k1", "Metric", weight=1, label="Revenue", value=p("/kpi/revenue"), prefix="$", suffix="M", decimals=2, change="+12%", context="vs last year"),
        x("k2", "Metric", weight=1, label="Deals won", value=p("/kpi/deals"), change="+31", context="vs last year"),
        x("k3", "Metric", weight=1, label="Win rate", value=p("/kpi/winRate"), suffix="%", decimals=1, change="+2.1", context="points"),
        x("k4", "Metric", weight=1, label="Cycle", value=p("/kpi/cycle"), suffix="d", change="-4d", context="median"),
        c("split", "Row", children=["trend_card", "mix_card"]),
        c("trend_card", "Card", weight=3, child="trend"),
        x("trend", "LineChart", title="Revenue vs target ($k)", labels=p("/months"), series=p("/series"), unit="$k", height=296),  # as tall as the donut card
        c("mix_card", "Card", weight=2, child="mix"),
        x("mix", "DonutChart", title="Revenue by channel", labels=p("/channels/labels"), values=p("/channels/values"), totalLabel="$k total"),
        c("deals_card", "Card", child="deals"),
        x("deals", "Table", caption="Largest open deals", columns=[{"key": "account", "title": "Account"}, {"key": "owner", "title": "Owner"}, {"key": "stage", "title": "Stage"}, {"key": "amount", "title": "Amount", "format": "currency:USD"}], rows=p("/deals")),
        c("actions", "Row", justify="end", children=["apply", "share"]),
        *button("apply", "Show region", event("filter_region", region=p("/region/0")), variant="default", checks=[required("/region", "Pick a region.")]),
        *button("share", "Share report", event("share_report"), primary=True),
    ]
    return answer("Here's the full-year revenue review. Pick a region to drill in:",
                  create(S, comps[:2], sales_data(1.0, "all regions", 0)),
                  update(S, comps[2:7]),
                  update(S, comps[7:12]),
                  update(S, comps[12:]))

def sales_region(name, scale, shift):
    S = "sales_1"
    d = sales_data(scale, name, shift)
    return answer(f"Here is {name}. Revenue, channels and the deal list now show that region only.",
                  data(S, "/regionName", d["regionName"]),
                  data(S, "/kpi", d["kpi"]),
                  data(S, "/series", d["series"]),
                  data(S, "/channels", d["channels"]),
                  data(S, "/deals", d["deals"]))

SALES = {
    "id": "sales", "title": "Sales dashboard",
    "description": "Metrics, a two-series line chart, a donut and a table that all refresh when you pick a region.",
    "prompt": "Build me a revenue review for this year with a region filter.",
    "start": sales_start(),
    "actions": {
        "filter_region:americas": sales_region("Americas", 0.52, 2),
        "filter_region:emea": sales_region("EMEA", 0.31, -1),
        "filter_region:apac": sales_region("APAC", 0.17, 3),
        "share_report": answer("Shared with #leadership. Anyone in the channel can open the live report.",
                               create("sales_shared", [c("root", "Card", child="ok"), x("ok", "Alert", tone="success", title="Report shared", text="Sent to #leadership · 14 people can view it")], send=False)),
    },
}

# ---------------------------------------------------------------- Incident
EVENTS = [
    {"id": "e1", "at": "2026-11-14T14:02:00Z", "title": "Error rate on checkout-api crossed 5%", "actor": "Monitor", "meta": "alert · checkout-api"},
    {"id": "e2", "at": "2026-11-14T14:04:00Z", "title": "Paged the payments on-call", "actor": "Pager", "meta": "Sam Ito"},
    {"id": "e3", "at": "2026-11-14T14:09:00Z", "title": "Connection pool exhausted on orders-db replica 2", "actor": "Sam Ito", "meta": "investigation"},
    {"id": "e4", "at": "2026-11-14T14:12:00Z", "title": "Deploy 2026.11.14-3 of orders-service correlates with the spike", "actor": "Sam Ito", "meta": "deploys"},
]
def incident_start():
    S = "incident_1"
    comps = [
        c("root", "Column", children=["alert", "stats", "stage_card", "body", "note", "actions"]),
        x("alert", "Alert", tone=p("/alert/tone"), title=p("/alert/title"), text=p("/alert/text")),
        c("stats", "Row", children=["err_card", "lat_card", "failed"]),
        c("err_card", "Card", weight=1, child="err"),
        c("lat_card", "Card", weight=1, child="lat"),
        x("err", "Sparkline", label="Error rate", values=p("/errors"), value="6.8%", change="+5.9 pts"),
        x("lat", "Sparkline", label="p95 latency", values=p("/latency"), value="1.9 s", change="+1.2 s"),
        x("failed", "Metric", weight=1, label="Orders failed", value=p("/failed"), context="last 30 minutes"),
        c("stage_card", "Card", child="stage"),
        x("stage", "Stepper", steps=[{"label": "Detect"}, {"label": "Triage"}, {"label": "Mitigate"}, {"label": "Resolve"}, {"label": "Review"}], current=p("/stage")),
        c("body", "Row", children=["timeline_card", "services_card"]),
        c("timeline_card", "Card", weight=3, child="timeline"),
        x("timeline", "Timeline", label="Incident timeline", events=p("/events")),
        c("services_card", "Card", weight=2, child="services_col"),
        c("services_col", "Column", children=["services_title", "services"]),
        text("services_title", "### Affected services"),
        c("services", "List", children={"componentId": "svc", "path": "/services"}),
        c("svc", "Row", align="center", children=["svc_name", "svc_status"]),
        c("svc_name", "Column", weight=1, children=["svc_title", "svc_owner"]),
        text("svc_title", fmt("**${name}**")),
        text("svc_owner", fmt("${owner} · ${impact}"), caption=True),
        x("svc_status", "Badge", text=p("status"), tone=p("tone")),
        c("note", "TextField", label="Note for the timeline", placeholder="What did you check or change?", value=p("/note")),
        c("actions", "Row", justify="spaceBetween", align="center", children=["runbook", "buttons"]),
        c("runbook", "Modal", trigger="runbook_link", content="runbook_body"),
        text("runbook_link", "*Open the runbook*"),
        c("runbook_body", "Column", children=["rb_title", "rb_steps"]),
        text("rb_title", "### Checkout API runbook"),
        text("rb_steps", "1. Check the orders-db pool. 2. Roll back the last orders-service deploy. 3. Watch the error rate for 10 minutes."),
        c("buttons", "Row", children=["escalate", "ack"]),
        *button("escalate", "Escalate", event("escalate"), variant="default"),
        *button("ack", "Acknowledge and roll back", {"event": {"name": "acknowledge", "context": {"note": p("/note")}}}, primary=True,
                checks=[required("/note", "Add a note first.")]),
    ]
    dm = {
        "alert": {"tone": "danger", "title": "Checkout API error rate above 5%", "text": "Started 14:02 UTC · 3 services affected · customers see failed payments"},
        "errors": [0.4, 0.5, 0.4, 0.6, 0.5, 0.7, 1.9, 4.2, 6.1, 6.8],
        "latency": [0.6, 0.6, 0.7, 0.6, 0.7, 0.8, 1.1, 1.6, 1.8, 1.9],
        "failed": 412, "stage": 1, "events": EVENTS, "note": "",
        "services": [
            {"name": "checkout-api", "owner": "Payments", "impact": "6.8% errors", "status": "Degraded", "tone": "danger"},
            {"name": "orders-service", "owner": "Orders", "impact": "pool exhausted", "status": "Degraded", "tone": "danger"},
            {"name": "notifications", "owner": "Platform", "impact": "delayed emails", "status": "Watching", "tone": "warning"},
        ],
    }
    return answer("This is an active incident. Here's what I know so far:",
                  create(S, comps[:8], dm),
                  update(S, comps[8:12]),
                  update(S, comps[12:22]),
                  update(S, comps[22:]))

INCIDENT = {
    "id": "incident", "title": "Incident response",
    "description": "Alert, sparklines, a stepper, a live timeline and service badges that change as you act.",
    "prompt": "Checkout is failing — show me the incident.",
    "start": incident_start(),
    "actions": {
        "acknowledge": answer("Rolled back orders-service and noted it on the timeline. The error rate is falling.",
                              data("incident_1", "/stage", 2),
                              data("incident_1", "/alert", {"tone": "warning", "title": "Mitigating: rollback in progress", "text": "Error rate 2.1% and falling · rollback 60% complete"}),
                              data("incident_1", "/events/4", {"id": "e5", "at": "2026-11-14T14:16:00Z", "title": "Acknowledged; rolling back orders-service to 2026.11.14-2", "actor": "You", "meta": "mitigation"}),
                              data("incident_1", "/services/1/status", "Rolling back"),
                              data("incident_1", "/services/1/tone", "warning"),
                              data("incident_1", "/errors/10", 2.1)),
        "escalate": answer("Paged the database on-call. They join the bridge in about 3 minutes.",
                           data("incident_1", "/events/4", {"id": "e5b", "at": "2026-11-14T14:15:00Z", "title": "Escalated to database on-call", "actor": "You", "meta": "Dana Kim"})),
    },
}

# ---------------------------------------------------------------- Checkout wizard
CART = [
    {"item": "Studio Buds", "qty": 2, "price": 149, "total": 298},
    {"item": "USB-C Hub", "qty": 1, "price": 59, "total": 59},
    {"item": "Laptop Sleeve 14\"", "qty": 1, "price": 39, "total": 39},
]
STEPS = [{"label": "Shipping", "description": "Address and speed"}, {"label": "Payment", "description": "Card details"}, {"label": "Review", "description": "Check and pay"}]
def checkout_start():
    S = "checkout_1"
    comps = [
        c("root", "Card", child="wizard"),
        c("wizard", "Column", children=["title", "steps", "main"]),
        c("main", "Row", children=["body", "summary_card"]),
        c("summary_card", "Card", weight=2, child="summary_col"),
        c("summary_col", "Column", children=["summary_title", "summary_items", "summary_total"]),
        text("summary_title", "### Order summary"),
        c("summary_items", "List", children={"componentId": "line", "path": "/cart"}),
        c("line", "Row", justify="spaceBetween", children=["line_name", "line_total"]),
        text("line_name", fmt("${qty} × ${item}"), caption=True),
        text("line_total", fmt("${formatCurrency(value: ${total}, currency: 'USD')}")),
        text("summary_total", fmt("**Total ${formatCurrency(value: ${/total}, currency: 'USD')}**")),
        text("title", "### Checkout · 3 items"),
        x("steps", "Stepper", steps=STEPS, current=p("/step")),
        c("body", "Column", weight=3, children=["name", "address", "delivery", "speed", "ship_row"]),
        c("name", "TextField", label="Full name", value=p("/ship/name"), __checks=[required("/ship/name", "Enter the recipient's name.")]),
        c("address", "TextField", label="Address", variant="longText", value=p("/ship/address"), __checks=[required("/ship/address", "Enter a delivery address.")]),
        c("delivery", "DateTimeInput", label="Deliver on", enableDate=True, min="2026-11-16", value=p("/ship/date")),
        c("speed", "ChoicePicker", label="Shipping", variant="mutuallyExclusive", displayStyle="chips", options=[{"label": "Standard · free", "value": "standard"}, {"label": "Express · $12", "value": "express"}, {"label": "Next day · $25", "value": "nextday"}], value=p("/ship/speed")),
        c("ship_row", "Row", justify="end", children=["to_payment"]),
        *button("to_payment", "Continue to payment", event("to_payment"), primary=True,
                checks=[{"condition": {"@call": "and", "args": {"values": [{"@call": "required", "args": {"value": p("/ship/name")}}, {"@call": "required", "args": {"value": p("/ship/address")}}]}}, "message": "Name and address are needed."}]),
    ]
    dm = {"step": 0, "ship": {"name": "", "address": "", "date": "2026-11-18", "speed": ["standard"]}, "pay": {"card": "", "expiry": "", "save": True}, "cart": CART, "subtotal": 396, "shipping": 0, "total": 396}
    return answer("Let's get your order out. First, where should it go?", create(S, comps, dm))

PAYMENT = [
    c("body", "Column", weight=3, children=["card", "expiry", "save", "pay_row"]),
    c("card", "TextField", label="Card number", variant="obscured", placeholder="16 digits", value=p("/pay/card"),
      __checks=[{"condition": {"@call": "regex", "args": {"value": p("/pay/card"), "pattern": "^[0-9]{16}$"}}, "message": "A card number has 16 digits."}]),
    c("expiry", "TextField", label="Expiry (MM/YY)", value=p("/pay/expiry"),
      __checks=[{"condition": {"@call": "regex", "args": {"value": p("/pay/expiry"), "pattern": "^(0[1-9]|1[0-2])/[0-9]{2}$"}}, "message": "Use MM/YY."}]),
    c("save", "CheckBox", label="Save this card for next time", value=p("/pay/save")),
    c("pay_row", "Row", justify="end", children=["to_review"]),
    *button("to_review", "Review order", event("to_review"), primary=True,
            checks=[{"condition": {"@call": "and", "args": {"values": [{"@call": "regex", "args": {"value": p("/pay/card"), "pattern": "^[0-9]{16}$"}}, {"@call": "regex", "args": {"value": p("/pay/expiry"), "pattern": "^(0[1-9]|1[0-2])/[0-9]{2}$"}}]}}, "message": "Check the card details."}]),
]
REVIEW = [
    c("main", "Row", children=["body"]),
    c("body", "Column", weight=1, children=["summary", "totals", "eta", "place_row"]),
    x("summary", "Table", caption="Your order", columns=[{"key": "item", "title": "Item"}, {"key": "qty", "title": "Qty", "numeric": True}, {"key": "price", "title": "Price", "format": "currency:USD"}, {"key": "total", "title": "Total", "format": "currency:USD"}], rows=p("/cart")),
    c("totals", "Row", justify="end", children=["totals_col"]),
    c("totals_col", "Column", align="end", children=["t_sub", "t_ship", "t_total"]),
    text("t_sub", fmt("Subtotal ${formatCurrency(value: ${/subtotal}, currency: 'USD')}"), caption=True),
    text("t_ship", fmt("Shipping ${formatCurrency(value: ${/shipping}, currency: 'USD')}"), caption=True),
    text("t_total", fmt("**Total ${formatCurrency(value: ${/total}, currency: 'USD')}**")),
    x("eta", "Alert", tone="info", title=fmt("Arrives ${formatDate(value: ${/ship/date}, format: 'EEEE, d MMMM')}"), text=fmt("Shipping to ${/ship/name}")),
    c("place_row", "Row", justify="end", children=["place"]),
    *button("place", "Place order", event("place_order"), primary=True),
]
DONE = [
    c("body", "Column", weight=1, children=["done", "tracking"]),
    x("done", "Alert", tone="success", title="Order A-20931 placed", text=fmt("A receipt is on its way. Total charged: ${formatCurrency(value: ${/total}, currency: 'USD')}")),
    x("tracking", "Timeline", label="Tracking", events=[
        {"id": "t1", "at": "2026-11-14T15:40:00Z", "title": "Order placed", "actor": "You"},
        {"id": "t2", "at": "2026-11-14T15:41:00Z", "title": "Payment confirmed", "actor": "Payments"},
        {"id": "t3", "at": "2026-11-14T15:55:00Z", "title": "Packing started at the Lisbon warehouse", "actor": "Warehouse"},
    ]),
]
CHECKOUT = {
    "id": "checkout", "title": "Checkout wizard",
    "description": "A three-step flow: validated forms, a date, shipping options, an order table with totals and tracking.",
    "prompt": "I'm ready to check out my cart.",
    "start": checkout_start(),
    "actions": {
        "to_payment": answer("Got it. Now the payment details:", data("checkout_1", "/step", 1), update("checkout_1", PAYMENT)),
        "to_review": answer("Here's everything before you pay:", data("checkout_1", "/step", 2), update("checkout_1", REVIEW)),
        "place_order": answer("Done! I'll keep an eye on the delivery.", data("checkout_1", "/step", 3), update("checkout_1", DONE)),
    },
}

# ---------------------------------------------------------------- Project board
TASKS = [
    {"id": "ARC-412", "title": "Streaming A2UI renderer", "assignee": "Mia Chen", "status": "In review", "points": 8},
    {"id": "ARC-415", "title": "Chat layout for narrow screens", "assignee": "Leo Park", "status": "In progress", "points": 5},
    {"id": "ARC-418", "title": "Provider settings and key storage", "assignee": "Ana Ruiz", "status": "In progress", "points": 5},
    {"id": "ARC-421", "title": "Conformance suite in CI", "assignee": "Sam Ito", "status": "To do", "points": 3},
    {"id": "ARC-422", "title": "Docs: Agents category", "assignee": "Mia Chen", "status": "Done", "points": 2},
    {"id": "ARC-425", "title": "WebAssembly deploy for a2ui.tkcode.si", "assignee": "Leo Park", "status": "To do", "points": 3},
]
def project_start():
    S = "project_1"
    comps = [
        c("root", "Column", children=["header", "tabs", "footer"]),
        c("header", "Row", align="center", children=["title_col", "people"]),
        c("title_col", "Column", weight=1, children=["title", "subtitle"]),
        text("title", "## Sprint 24 · A2UI launch"),
        text("subtitle", "Nov 10 – Nov 21 · 6 tasks · 26 points", caption=True),
        c("people", "Row", children=["a1", "a2", "a3", "a4"]),
        x("a1", "Avatar", name="Mia Chen"), x("a2", "Avatar", name="Leo Park"), x("a3", "Avatar", name="Ana Ruiz"), x("a4", "Avatar", name="Sam Ito"),
        c("tabs", "Tabs", tabs=[{"title": "Overview", "child": "overview"}, {"title": "Tasks", "child": "tasks_tab"}, {"title": "Team", "child": "team_tab"}]),
        c("overview", "Column", children=["ov_metrics", "milestones_card"]),
        c("milestones_card", "Card", child="milestones_col"),
        c("milestones_col", "Column", children=["milestones_title", "milestones"]),
        c("ov_metrics", "Row", children=["m_open", "m_done", "velocity_card"]),
        c("velocity_card", "Card", weight=1, child="velocity"),
        x("m_open", "Metric", weight=1, label="Open points", value=p("/open"), change="-5", context="since Monday"),
        x("m_done", "Metric", weight=1, label="Done this week", value=p("/done"), change="+7"),
        x("velocity", "Sparkline", label="Velocity", values=[18, 21, 19, 24, 22, 26], value="26 pts", change="+4"),
        text("milestones_title", "### Milestones"),
        c("milestones", "List", children={"componentId": "ms", "path": "/milestones"}),
        c("ms", "Column", children=["ms_head", "ms_bar", "ms_gap"]),
        c("ms_gap", "Divider"),
        c("ms_head", "Row", align="center", children=["ms_name", "ms_badge"]),
        text("ms_name", fmt("**${name}** · due ${due}")),
        x("ms_badge", "Badge", text=p("state"), tone=p("tone")),
        x("ms_bar", "Progress", value=p("done"), max=100),
        c("tasks_tab", "Column", children=["filter_row", "tasks"]),
        c("filter_row", "Row", align="end", children=["assignee", "filter_btn"]),
        c("assignee", "ChoicePicker", label="Assignee", variant="mutuallyExclusive", filterable=True, weight=1, options=[{"label": "Mia Chen", "value": "mia"}, {"label": "Leo Park", "value": "leo"}, {"label": "Ana Ruiz", "value": "ana"}, {"label": "Sam Ito", "value": "sam"}], value=p("/assignee")),
        *button("filter_btn", "Filter", event("filter_tasks", who=p("/assignee/0")), variant="default"),
        x("tasks", "Table", caption="Sprint backlog", columns=[{"key": "id", "title": "Key"}, {"key": "title", "title": "Task"}, {"key": "assignee", "title": "Assignee"}, {"key": "status", "title": "Status"}, {"key": "points", "title": "Points", "numeric": True}], rows=p("/tasks")),
        c("team_tab", "List", children={"componentId": "member", "path": "/team"}),
        c("member", "Row", align="center", children=["member_avatar", "member_text", "member_load"]),
        x("member_avatar", "Avatar", name=p("name")),
        c("member_text", "Column", weight=1, children=["member_name", "member_role"]),
        text("member_name", p("name")),
        text("member_role", p("role"), caption=True),
        x("member_load", "Badge", text=fmt("${points} pts"), tone=p("tone")),
        c("footer", "Row", justify="end", children=["plan"]),
        *button("plan", "Plan next sprint", event("plan_sprint"), primary=True),
    ]
    dm = {
        "open": 16, "done": 10, "assignee": [], "tasks": TASKS,
        "milestones": [
            {"name": "Renderer", "due": "Nov 14", "done": 100, "state": "Done", "tone": "success"},
            {"name": "Chat app", "due": "Nov 19", "done": 70, "state": "On track", "tone": "info"},
            {"name": "Deploy", "due": "Nov 21", "done": 20, "state": "At risk", "tone": "warning"},
        ],
        "team": [
            {"name": "Mia Chen", "role": "Renderer lead", "points": 10, "tone": "warning"},
            {"name": "Leo Park", "role": "Frontend", "points": 8, "tone": "info"},
            {"name": "Ana Ruiz", "role": "Platform", "points": 5, "tone": "success"},
            {"name": "Sam Ito", "role": "Infrastructure", "points": 3, "tone": "success"},
        ],
    }
    return answer("Here's the sprint board. Switch tabs for tasks and the team:",
                  create(S, comps[:12], dm),
                  update(S, comps[12:27]),
                  update(S, comps[27:]))

def filtered(who, name):
    return answer(f"Showing {name}'s tasks.", data("project_1", "/tasks", [t for t in TASKS if t["assignee"] == name]))

PROJECT = {
    "id": "project", "title": "Project board",
    "description": "Tabs with metrics, milestone progress, a filterable task table and a team list.",
    "prompt": "Show me the sprint board for the A2UI launch.",
    "start": project_start(),
    "actions": {
        "filter_tasks:mia": filtered("mia", "Mia Chen"),
        "filter_tasks:leo": filtered("leo", "Leo Park"),
        "filter_tasks:ana": filtered("ana", "Ana Ruiz"),
        "filter_tasks:sam": filtered("sam", "Sam Ito"),
        "filter_tasks": answer("Showing every task.", data("project_1", "/tasks", TASKS)),
        "plan_sprint": answer("Here's a draft for sprint 25 based on what's left and the team's load:",
                              create("sprint_25", [
                                  c("root", "Card", child="plan"),
                                  c("plan", "Column", children=["plan_title", "plan_steps", "plan_note"]),
                                  text("plan_title", "### Sprint 25 draft"),
                                  x("plan_steps", "Stepper", orientation="vertical", current=1, steps=[
                                      {"label": "Carry over", "description": "ARC-421, ARC-425 (6 pts)"},
                                      {"label": "New work", "description": "Arc catalog charts, list virtualization (13 pts)"},
                                      {"label": "Buffer", "description": "4 pts for bugs from the launch"}]),
                                  x("plan_note", "Alert", tone="info", title="23 points planned", text="Within the last three sprints' average velocity of 24.")], send=False)),
    },
}

# ---------------------------------------------------------------- Webhook setup (Markdown, CodeBlock, Accordion, Switch, SegmentedControl, Rating)
INTRO = """## Webhooks for Lumen Pay
Webhooks tell your server about events **as they happen**, so you do not poll the API.

1. Pick the environment and the events below.
2. We create the endpoint and give you a signing secret.
3. Verify each request's `Lumen-Signature` header before you trust it.

See the [webhook reference](https://docs.example.com/webhooks) for every event type."""

CURL = """curl https://api.lumenpay.example/v1/webhooks \\
  -H "Authorization: Bearer $LUMEN_KEY" \\
  -d url=https://shop.example.com/hooks/lumen \\
  -d "events[]=payment.succeeded" \\
  -d "events[]=refund.created\""""
CSHARP = """app.MapPost("/hooks/lumen", async (HttpRequest request) =>
{
    var body = await new StreamReader(request.Body).ReadToEndAsync();
    var signature = request.Headers["Lumen-Signature"].ToString();
    if (!LumenSignature.Verify(body, signature, secret))
    {
        return Results.Unauthorized();
    }

    var evt = JsonSerializer.Deserialize(body, LumenJson.Default.LumenEvent);
    // ... handle evt.Type
    return Results.Ok();
});"""
PYTHON = """@app.post("/hooks/lumen")
async def lumen_hook(request: Request):
    body = await request.body()
    if not lumen.verify(body, request.headers["Lumen-Signature"], SECRET):
        raise HTTPException(status_code=401)
    event = json.loads(body)
    # ... handle event["type"]
    return {"ok": True}"""

def webhooks_start():
    S = "webhooks_1"
    comps = [
        c("root", "Column", children=["intro_card", "setup_card", "actions"]),
        c("intro_card", "Card", child="intro"),
        x("intro", "Markdown", text=INTRO),
        c("setup_card", "Card", child="setup"),
        c("setup", "Column", children=["setup_title", "env_row", "events_title", "ev1", "ev2", "ev3"]),
        text("setup_title", "### Endpoint"),
        c("env_row", "Row", align="center", justify="spaceBetween", children=["env_label", "env"]),
        text("env_label", fmt("Environment · **${/env}**")),
        x("env", "SegmentedControl", options=[{"label": "Test", "value": "test"}, {"label": "Live", "value": "live"}], value=p("/env")),
        text("events_title", "Events to send", caption=True),
        x("ev1", "Switch", label="payment.succeeded", value=p("/events/payment")),
        x("ev2", "Switch", label="refund.created", value=p("/events/refund")),
        x("ev3", "Switch", label="dispute.opened", value=p("/events/dispute")),
        c("actions", "Row", justify="end", children=["create"]),
        *button("create", "Create endpoint", {"event": {"name": "create_endpoint", "context": {"env": p("/env")}}}, primary=True),
    ]
    dm = {"env": "test", "events": {"payment": True, "refund": True, "dispute": False}, "stars": 0}
    return answer("Here's how webhooks work, with the setup right below:",
                  create(S, comps[:4], dm),
                  update(S, comps[4:]))

def endpoint(env):
    S = "webhooks_" + env
    secret = "whsec_test_8f2c41d07a" if env == "test" else "whsec_live_51b9e6c3f0"
    comps = [
        c("root", "Column", children=["ok", "secret_title", "secret", "samples_title", "samples", "feedback_card"]),
        x("ok", "Alert", tone="success", title=f"Endpoint created in {env} mode",
          text="Lumen will POST payment.succeeded and refund.created to https://shop.example.com/hooks/lumen"),
        text("secret_title", "### Signing secret"),
        x("secret", "CodeBlock", code=secret, language="text", fileName=".env"),
        text("samples_title", "### Verify and handle the request"),
        x("samples", "Accordion", expanded=0, items=[
            {"title": "C# (ASP.NET Core)", "child": "code_cs"},
            {"title": "Python (FastAPI)", "child": "code_py"},
            {"title": "Register with cURL", "child": "code_curl"}]),
        x("code_cs", "CodeBlock", code=CSHARP, language="csharp", fileName="Program.cs"),
        x("code_py", "CodeBlock", code=PYTHON, language="python", fileName="hooks.py"),
        x("code_curl", "CodeBlock", code=CURL, language="bash"),
        c("feedback_card", "Card", child="feedback"),
        c("feedback", "Row", align="center", children=["feedback_text", "stars", "send"]),
        text("feedback_text", "Was this guide clear?"),
        x("stars", "Rating", weight=1, value=p("/stars")),
        *button("send", "Send", {"event": {"name": "rate_guide", "context": {"stars": p("/stars")}}}, variant="default",
                checks=[{"condition": {"@call": "numeric", "args": {"value": p("/stars"), "min": 1}}, "message": "Pick 1 to 5 stars."}]),
    ]
    return answer(f"Done. Keep the secret out of source control; the samples below verify each request with it.",
                  # The setup card says it is done instead of offering the button again.
                  update("webhooks_1", [c("actions", "Row", justify="end", children=["created"]),
                                        x("created", "Badge", text=f"Endpoint created · {env}", tone="success")]),
                  create(S, comps, {"stars": 0}))

WEBHOOKS = {
    "id": "webhooks", "title": "Webhook setup",
    "description": "A Markdown guide, a segmented control and switches, then code samples in an accordion and a star rating.",
    "prompt": "Help me set up payment webhooks.",
    "start": webhooks_start(),
    "actions": {
        "create_endpoint:test": endpoint("test"),
        "create_endpoint:live": endpoint("live"),
        "rate_guide": answer("Thanks for the rating! I'll keep the samples this short."),
    },
}

ALL = [SALES, INCIDENT, CHECKOUT, PROJECT, WEBHOOKS]

def cs(s): return '"""\n' + s + '"""'
out = ["// <auto-generated> by a2ui/tools/gen_rich_showcases.py; edit the script, not this file. </auto-generated>",
       "namespace TKCode.Arc.A2UI.Chat;", "",
       "public static partial class ShowcaseLibrary", "{",
       "    /// <summary>The rich examples: dashboards and multi-step flows built from the basic and the chat catalogs.</summary>",
       "    public static IReadOnlyList<Showcase> Rich { get; } =", "    ["]
for s in ALL:
    acts = "\n".join(f'                ["{k}"] = """\n{v}"""' + ("," if i < len(s["actions"]) - 1 else "") for i, (k, v) in enumerate(s["actions"].items()))
    out.append(f'''        new Showcase
        {{
            Id = "{s["id"]}",
            Title = "{s["title"]}",
            Description = "{s["description"]}",
            Prompt = "{s["prompt"]}",
            Start = """
{s["start"]}""",
            OnAction = new Dictionary<string, string>
            {{
{acts}
            }}
        }},''')
out.append("    ];")
out.append("}")
path = pathlib.Path(__file__).resolve().parents[1] / "TKCode.Arc.A2UI.Chat" / "Showcases" / "ShowcaseLibrary.Rich.g.cs"
path.write_text("\n".join(out) + "\n", encoding="utf-8")
print("wrote", path)
