---
title: Code review
summary: A pull request review in one card: file tabs that track what you have viewed, a syntax-colored diff where any line takes a comment, conversations that fold to one line once resolved, suggestions applied straight into the diff, and an Approve button that turns into Approved.
---

<Demo name="Preview" />

## When to use

`CodeReview` puts a whole pull request review in one card, for tools where people check changes before they ship:

- **Reviewing a pull request** with comments on lines and suggested changes.
- **Approving generated changes** file by file in an internal tool.
- **A review queue** where it matters which files were read and which conversations are still open.

## When not to use

- **Showing one snippet that nobody comments on.** Use `CodeBlock`.
- **Following an agent while it writes code.** Use the Agent run block.
- **Comparing two prose documents** without review actions; a plain diff view is enough.

## Usage

The card derives from Avalonia's `TemplatedControl` and needs no data: with no property set it shows a sample pull
request with five files, a resolved conversation and an open one that suggests a change, and it simulates the approval.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:CodeReview Pull="{Binding Pull}" Files="{Binding Files}" Threads="{Binding Threads}" />
</UserControl>
```

### The data

- `Pull` is a `CodeReviewPullRequest`: title, number, author, base and head branch and commit count, shown in the header.
- `Files` lists the changed files, one tab each in this order. A `CodeReviewFile` has an id, a path, a language for the
  colors (`ts`, `md`, `csharp`, anything the code highlighter knows) and its diff as `CodeReviewDiffLine`s: a hunk
  header with the first old and new line numbers, followed by unchanged, added and removed lines. The line numbers are
  derived from the hunk headers, so a rewritten line renumbers the rest.
- `Threads` are the existing conversations. Each `CodeReviewThread` hangs under a line by its id (not its number, so it
  stays in place when a suggestion rewrites the file), has a label such as "Line 9", its comments, whether it is
  resolved, and an optional `CodeReviewSuggestion` with the ids of the lines it replaces and the new text.
- `Viewer` is the person reviewing; new comments and replies are written as them.

The card shows the first file with an open conversation; `SelectedFileId` chooses another and follows the tabs.

### Reviewing

- **Comment on a line**: point at a line and a plus appears on the edge of its number; click it (or focus the number and
  press Enter) to open a comment box under the line. "Suggest a change" adds a code field filled with the line. The
  new thread reads "Pending" until the review is approved, and `CommentAdded` reports it.
- **Threads**: "Reply" opens a reply box in place. "Resolve" folds the thread to one line with its participants; click
  the line to open it again, then "Hide" or "Reopen". `ReplyPosted` and `ResolvedChanged` report each change.
- **Suggestions**: "Apply suggestion" replaces the lines in the diff, marks the thread resolved and folds it a moment
  later. `SuggestionApplied` hands over the file's new diff so your server can commit it.
- **Viewed**: the checkbox in the file header marks the file read; its tab shows a check and the progress fills.

### Connecting your server

`SubmitReviewAction` sends the approval with the comments added during the review. The button shows progress while the
task runs; an exception turns it into "Try again". After a successful send the button reads "Approved", a status line
opens under the header and `ReviewSubmitted` is raised; "Undo" withdraws it and raises `ApprovalWithdrawn`. Without an
action the send is simulated.

```csharp
review.SubmitReviewAction = async (submission, cancellation) =>
{
    var response = await http.PostAsJsonAsync($"/api/pulls/{pull.Number}/reviews",
        new { body = submission.Body, comments = submission.Comments.Select(t => t.Comments[0].Text) }, cancellation);
    response.EnsureSuccessStatusCode();
};

review.SuggestionApplied += (_, e) => commits.Queue(e.File.Path, e.Lines);
```

### Texts

Every word the card shows or announces is a property of `CodeReviewLabels`; assign a new instance to `Labels` to
translate or reword. Texts with numbers or names are functions, so the word order can change.

## API reference

<!-- api: CodeReview -->

<!-- api: CodeReviewFile -->

<!-- api: CodeReviewThread -->

## Accessibility

- The card is named after the pull request ("Review of pull request 3172") and the title is a heading.
- The files are a tab list named "Changed files" with one tab stop; each tab reads its path, whether it is viewed and how
  many conversations are open ("src/mail/limiter.ts, viewed, 1 open conversation").
- Every line number is a button named after the line ("Comment on added line 9", "Comment on deleted line 27"), so the
  diff reads correctly without its colors.
- Viewed and "Suggest a change" are check boxes that report their state; a folded thread is a button that opens it.
- Adding comments and replies, resolving, applying suggestions, marking files viewed and approving are announced in a
  polite live region.

### Keyboard

- **Tab** moves through the Approve button, the file tabs, the Viewed checkbox, one line number per file and the thread
  actions.
- **Left** / **Right**, **Home** / **End** in the tabs choose the previous, next, first or last file.
- **Up** / **Down**, **Home** / **End** on a line number move to the previous, next, first or last line.
- **Enter** or **Space** on a line number opens or closes its comment box.
- **Ctrl** + **Enter** (**Cmd** + **Enter** on macOS) posts the comment or reply being written; **Escape** closes the box
  and returns to the line number.

## Motion

- **Bands**: comment boxes, threads and the approval status open in the flow of the diff; their height follows the
  content on one critically damped spring (about 420 ms), so the lines below slide instead of jumping, and the content
  fades in.
- **Posting**: the box keeps its band and becomes the new thread while the height springs between them.
- **Applying**: the old line folds out while the new one grows in at the same place with a stronger green that holds,
  then eases to the normal added color; "Apply suggestion" turns into "Applied" and the thread folds 650 ms later.
- **Add-comment mark**: scales in from 60% on the gutter's edge 70 ms after the pointer arrives, so a passing pointer
  lights nothing; the line numbers brighten with it.
- **Tabs**: the highlight slides between files, the panel crossfades with a 16 px slide in the direction of travel and
  its height springs; the viewed progress fills on the same spring with a rolling count.
- **Approve** shows a spinner, then its label and width change to Approved and the status line opens under the header.
- Reduced motion applies every change at once and keeps short fades; Off applies everything at once. Focus and
  announcements are the same at every level.

## Responsive behavior

- `CodeReview` changes layout at its own width. Below `CompactWidth` (560 px by default) it sets `:compact`: the header
  stacks with Approve under the title, the progress moves above the tabs, Viewed shows only its box and one line-number
  column remains.
- The card is at most 760 px wide. Tabs scroll sideways when they do not fit. Long lines never wrap: the code scrolls
  sideways (Shift + wheel, a horizontal wheel or the bar under the diff) while the line numbers stay pinned and threads
  keep the width of the card.
- Touch: the add-comment mark does not depend on hover; tapping a line number opens the box.

## Related

- `CodeBlock`, the code highlighting the diff uses (installed with `UseArcHighlighting()`).
- `Tabs`, `Checkbox`, `Button`, `Textarea` and `AnimatedCounter`, the controls whose patterns the card follows.
- API keys, Webhooks, Integrations, Import wizard, Env variables, Domain verify and Webhook tester, the other Developer
  blocks.
