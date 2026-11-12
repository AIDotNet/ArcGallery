---
title: Import wizard
summary: A four step CSV import in one card: drop a file and preview it, match its columns to your fields with a confidence for each guess, fix or clean up the cells with problems while the counts roll down, then import in batches and read the summary.
---

<Demo name="Preview" />

## When to use

`ImportWizard` brings rows from a spreadsheet export into your product. People drop a CSV, check that the columns landed
in the right fields, fix what is wrong and import the rest:

- **Importing contacts, customers, products or users** from a CSV that another tool exported.
- **Onboarding** that needs a customer's existing data before the product is useful.
- **Admin tools that update many records at once**, where existing keys become updates instead of duplicates.

## When not to use

- **Attaching files to a record.** Use `FileUpload` or `FileDropzone`; nothing is parsed there.
- **Mapping columns without checking the values.** A form with one `Select` per column is enough.
- **Keeping data in sync with another tool.** Use an integration that syncs on a schedule; a wizard is a one-off import.

## Usage

The card derives from Avalonia's `TemplatedControl` and needs no data: with no property set it uses a contact schema
(name, email, company, role, phone, country, plan), offers a sample customer list behind "Try a sample file" and
simulates the import.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:arc="https://github.com/AIDotNet/TKCode.Arc">
    <arc:ImportWizard BatchSize="200" Noun="contact" PluralNoun="contacts" />
</UserControl>
```

### The four steps

`Step` tells which step is shown, and `StepChanged` reports every change. Done steps in the step bar jump back to that
step; nothing can jump while the import runs.

1. **Upload**: drop a CSV on the zone, click it (or press Enter) to pick one, or load the sample. The file is read with
   a short progress line, then its name, size, row and column counts and the first five rows show. Files with fewer than
   two columns, no data row, an unclosed quote or more than `MaxRows` rows are refused with a message under the zone.
2. **Match fields**: every column of the file gets a row with two sample values and a field select. The columns resolve
   one after another, each with a confidence: 90 and above in green, 80 to 89 in neutral, below that in amber. A field
   picked by hand reads "Chosen", a column left out reads "Ignored", and a field picked for two columns reads
   "Duplicate" on both. Every required field needs exactly one column before "Check rows" is enabled.
3. **Review**: the rows with problems are listed first ("To fix"), each bad cell tinted with its message under the value.
   Click a cell (or press Enter) to edit it; Enter or leaving the cell saves, Escape cancels. "Clean up n values" applies
   every safe fix one cell at a time. The x at the end of a row leaves the row out. Rows that still have a problem are
   left out of the import, not blocking it.
4. **Import**: the ready rows go out in batches of `BatchSize` with a progress bar, then the summary shows how many were
   new, updated (their key was in `ExistingKeys`) and left out. "Download left-out rows" saves those rows as a CSV, and
   "Start a new import" begins again.

### Connecting your server

`ImportBatchAction` receives each batch and a cancellation token that Cancel, starting over and the card leaving the
window cancel. An exception stops the import, shows its message with "Try again" and raises `Failed`; the retry sends
the failed batch again. `Completed` (and `CompletedCommand`) report the totals once every batch is done.

```csharp
wizard.ImportBatchAction = async (records, cancellation) =>
{
    var response = await http.PostAsJsonAsync("/api/contacts/bulk", records.Select(r => r.Values), cancellation);
    response.EnsureSuccessStatusCode();
};

wizard.Completed += (_, e) => toast.Show($"{e.Created + e.Updated} contacts imported");
```

<Demo name="Handlers" />

### Your own fields

Describe the fields as `ImportTarget`s. `Headers` are names that mean the field exactly, `Synonyms` usual alternatives,
and `Sniff` recognises the values when the header gives no hint. `Validate` returns a short message for a wrong value;
`Fix` returns a safe cleaned-up value (trimmed, lower-cased, spelled out) or null, and powers "Clean up". Mark the field
that identifies an existing record with `IsKey`, and pass the known keys as `ExistingKeys`. `Noun` and `PluralNoun` name
the records in the copy, and `SampleCsv` replaces the sample (null hides the button).

<Demo name="CustomFields" />

The matcher scores a header that equals the field's label, key or one of its `Headers` at 99, a synonym backed by
values that look right at 95, a synonym alone at 88, values alone at 71 and a shared word at 64. Each field goes to
the column with the best score.

### Narrow containers

The card fills its container up to 760 wide. Below `CompactWidth` (520) the step bar shows "Step 1 of 4" with the
marks only, the mapping rows stack their select under the column name, and the bar above the review table wraps. The
preview and the review table scroll sideways inside their frames.

<Demo name="Narrow" />

### Texts

Every word the card shows or announces is a property of `ImportWizardLabels`; assign a new instance to `Labels` to
translate or reword. Texts with numbers are functions that return a format: `{0}` and `{1}` mark where the rolling
numbers go and `{noun}` takes the singular or plural noun.

## API reference

<!-- api: ImportWizard -->

<!-- api: ImportTarget -->

<!-- api: ImportRecord -->

<!-- api: ImportWizardCompletedEventArgs -->

## Accessibility

- The step bar is a list named "Import steps"; each step button reads its name, its number and whether it is done
  ("Upload, step 1 of 4, done"), and the current step is marked.
- The drop zone is a button named "Choose a CSV file"; the preview and the review table are focusable regions that
  scroll sideways with the arrow keys.
- Each field select is named after its column ("Field for Email"). A confidence is written as a number, not only shown
  as a bar, and "Chosen", "Ignored" and "Duplicate" are words.
- A cell with a problem carries the message in its name ("Email, line 3: Check the email format. Press Enter to edit").
- The review's counts and the import's progress are polite live regions; errors (a file that cannot be read, a missing
  field, a failed batch) are announced at once. The progress bar reports its current value and total.

### Keyboard

- **Tab** moves through the done steps of the step bar, the step's controls and its footer.
- **Enter** or **Space** on the drop zone opens the file picker.
- **Enter** or **Space** on a review cell opens its editor; **Enter** saves and **Escape** cancels, and the focus
  returns to the cell.

## Motion

- **Card**: its height follows each step on the smooth spring. Steps slide 28 px and blur 4 px in the direction of
  travel while the next one slides in from the other side.
- **Upload**: the drop zone shrinks away as the file card rises in; a 2 px line reads the file over 420 ms, then the five
  preview rows rise one after another, 40 ms apart.
- **Matching**: each column pulses "Matching" until it resolves, one every 117 ms; its score sharpens in and its meter
  fills on the smooth spring.
- **Review**: "Clean up" fixes one cell every 90 ms so the counts roll down in a cascade; fixed cells glow green for
  900 ms and, in "To fix", their rows slide out together before the rest close up.
- **Import**: the count and the bar follow every batch; the summary's numbers count up once and its check draws itself.
- Reduced motion keeps the fades, drops the travel, the reading line and the cascades, and shows the final numbers at
  once. Off applies every change at once.

## Responsive behavior

- `ImportWizard` changes layout at its own width, not the window's. Below `CompactWidth` (520 px by default) it sets
  `:compact`: the step bar shows the counter and bare marks, mapping rows stack and the review bar wraps.
- The card is at most 760 px wide (`MaxWidth`) at every size.
- File names and long cell values are trimmed with an ellipsis; tables scroll sideways inside their frames, so the page
  never scrolls sideways.
- Touch: nothing depends on hover. Cells open their editor on a tap, and every action is a visible button.

## Related

- `FileDropzone`, `Select`, `SegmentedControl`, `Progress`, `AnimatedCounter` and `InlineEdit`, the controls whose
  patterns the card follows.
- File upload, for attaching files rather than importing their rows.
- API keys, Webhooks, Integrations, Env variables, Domain verify and Webhook tester, the other Developer blocks.
