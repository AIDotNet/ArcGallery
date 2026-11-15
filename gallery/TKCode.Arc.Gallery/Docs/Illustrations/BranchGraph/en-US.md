---
title: Branch graph
summary: A git commit graph that previews how a pull request lands; branches draw in as colored lanes, a rebase lifts commits and replays them, a squash folds them into one, and hovering a commit shows its details.
---

<Demo name="PullRequest" />

## When to use

`BranchGraph` shows a short git history the way git clients do, newest commit on top: each branch in its own colored
lane, an S-bend where a branch leaves and where it joins back, merge commits drawn as rings with a merge icon, and a
pill on each branch's newest commit. The strategy control in the header previews how the pull request would land:

- **Before** is the history as it is, and the summary says how far the branch is ahead and behind.
- **Merge** makes room at the top for a merge commit and draws both lines into it.
- **Rebase** lifts the pull request's commits like cards, carries them one after another along an arc into the
  target's lane and sets them down with a ring while their hashes are rewritten.
- **Squash** flies the commits into one point, where a single commit named after the pull request appears.

Hovering a commit, or reaching it with the arrow keys, shows a card with its message, author and diff stats and dims
the other branches.

- **Code review and pull request screens**: a merge strategy picker that shows what the target will look like.
- **Developer tools**: a commit panel with hover details and keyboard browsing.
- **Docs and landing pages** for version control or CI products that explain merge, rebase and squash.

## When not to use

- **Long histories**: every commit is a row and nothing is virtualized. The graph is built for a focused slice of about
  five to fifteen commits; use a `Table` for a full log.
- **Release notes**: use a `Timeline` when people need to read what changed rather than how branches joined.
- **The live state of a repository**: the merge, rebase and squash views are previews computed from the data. Say so
  where the page could be read as the real thing.

## Usage

With no commits the graph previews an invented pull request (a feature branch three commits ahead of `main`, with a
short fix branch merged earlier). On first sight the lines draw up from the oldest commit; then, while nobody
interacts, it cycles Before, Rebase, Merge and Squash, waiting 2.6 seconds after each change settles. A pointer resting
on the commits holds the cycle, and the first pick, tap or key press stops it for good.

```xml
<arc:BranchGraph />
```

### Merge preview

Add `BranchGraphCommit` items oldest first; the graph shows the newest on top. `Branch` picks the lane and `Parents`
names a commit's parents (two make a merge). `Hash`, `Date`, `Author`, `Body`, `Additions`, `Deletions` and `Files`
fill the row and the detail card. `PullRequestTitle`, `PullRequestNumber`, `SourceBranch` and `TargetBranch` describe the
pull request; without them the graph takes the newest branch that has not been merged and lands it on the first branch.
Set `AutoPlay="False"` for a panel the reader drives.

<Demo name="MergePreview" />

```xml
<arc:BranchGraph PullRequestTitle="Article search" PullRequestNumber="1024"
                 SourceBranch="feat/search" AutoPlay="False">
    <arc:BranchGraphCommit Id="a41c0e7" Branch="main" Message="Publish the help center" Date="2 days ago" />
    <arc:BranchGraphCommit Id="6d2e9b1" Branch="feat/search" Message="Index articles on save" Date="Yesterday" />
    <arc:BranchGraphCommit Id="f03b58c" Branch="main" Parents="a41c0e7" Message="Upgrade to .NET 10" Date="3 hours ago" />
    <arc:BranchGraphCommit Id="b7a4e20" Branch="feat/search" Message="Rank results by freshness" Date="1 hour ago" />
</arc:BranchGraph>
```

`SelectedStrategy` (two way) is the preview shown and `SelectionChanged` reports every change with `OldValue`,
`NewValue` and `IsUserChange`. `SelectedCommit` (two way) is the commit whose card is pinned. `ItemsSource` takes the
commits from a view model, `CommitDrawn` is raised as each commit lands in the first-load sweep and `RevealedCount`
counts them, and `StartsFinished` opens with everything drawn and no cycle. `Labels` holds every word the graph shows or
speaks.

The cycle runs on the explainer clock of `Explainer`: `IsPlaying` is true while it cycles, `Pause()` holds it,
`StepForward()` moves to the next strategy, `Restart()` draws the graph in again, `Speed` scales every animation (0 shows
each state at once) and `AreControlsVisible` shows the strategy control.

## API reference

<!-- api: BranchGraph -->

<!-- api: BranchGraphCommit -->

<!-- api: BranchGraphStrategy -->

<!-- api: BranchGraphSelectionChangedEventArgs -->

<!-- api: BranchGraphLabels -->

<!-- api: BranchGraphCommitEventArgs -->

`BranchGraphSamples.CreateCommits()` returns a fresh copy of the sample history.

## Accessibility

- The card is a group named from the title and the branches ("Search filters, history of feat/filters and main").
- The strategy control is a row of segments with one tab stop; the arrow keys, Home and End pick a strategy at once.
- The commits are one tab stop, newest first. Arriving with the keyboard inspects the newest commit; Up and Down move
  through the commits, Home and End go to the first and last, Escape closes the card. The list's accessible name is the
  inspected commit with its hash, author, date, diff stats and the branches that point to it, or the summary.
- A polite live region speaks the summary after each strategy the reader picks; the cycle is not announced.
- Nothing relies on color alone: branch names are written in the pills, merge commits have a ring and an icon, and the
  diff stats carry + and − signs.

## Motion

| Change | Full | Reduced | Off |
|---|---|---|---|
| First sight | Lines draw up from the oldest commit; each commit lands as its line reaches it | Shown at once | Shown at once |
| Rebase and squash | Commits lift into cards and fly along an arc, 0.15 s apart, set down with a ring | Jump | Jump |
| Merge | Rows make room, the merge commit lands, both lines draw into it | Jump | Jump |
| Hash rewrite | The new hash rises in the accent color and fades to muted | In place | In place |
| Inspecting a commit | The other branches dim, the dot grows, the card fades in | At once | At once |
| Autoplay cycle | Runs | Off | Off |

Every pick starts from wherever things are, so picking a new strategy mid-flight never jumps. `Speed` multiplies the pace.

## Responsive behavior

- The graph keeps an 80 px gutter for up to three lanes; rows are 44 px and the message is cut with an ellipsis.
- The date column hides under 720 px, the author under 560 px and the hash under 400 px; the card and the accessible
  names keep them. Under 600 px the strategy control takes a full row.
- A tap on a commit pins its card and a tap on it again, or elsewhere in the list, closes it.

## Related

| Component | Choose it when |
|---|---|
| `Timeline` | Events in order, without branches |
| `ReasoningTree` | The story is a search that branches and prunes |
| `AgentTimeline` | Several agents working over time |
