---
title: Form
summary: A light form scope: labelled items with required marks and help text that show the view model's validation errors when you choose, and a submit that scrolls to the first invalid field.
---

<Demo name="Preview" />

## When to use

- **Sign-up, checkout, settings and dialogs** where several fields are submitted together and the person should see what
  to fix before anything is sent.
- **Rules that already live in the view model** (`INotifyDataErrorInfo`, for example CommunityToolkit.Mvvm's
  `ObservableValidator` with DataAnnotations): the form shows their errors at the right moment and in the right place.
- **One layout for every field**: the same label, required mark, help text and error row, labels above or in an
  aligned column on the left.

## When not to use

- **A single field**: an Arc text field already has a label, a description and an error row.
- **Rules written in XAML**: the form has no rule engine. Required, email, length, range and regular-expression rules
  belong to the view model.
- **Editing an object property by property**: use `PropertyGrid`.
- **A form spread over steps**: use the `MultiStepForm` block.

## Usage

`Form` derives from Avalonia's `ContentControl` and lays out nothing itself: put a panel inside. Every `FormItem` in it
joins the nearest form when it enters the logical tree and leaves it when it is removed, so there is nothing to
register. A `FormItem` derives from `Field`: `Label`, `Description` and `ErrorMessage` work as on any Arc field.

```xml
<arc:Form ValidateCommand="{Binding ValidateCommand}"
          SubmitCommand="{Binding CreateAccountCommand}"
          ResetCommand="{Binding ResetCommand}">
    <StackPanel Spacing="20">
        <arc:FormItem Label="Email" IsRequired="True">
            <arc:Input Text="{Binding Email}" Placeholder="you@example.com" />
        </arc:FormItem>
        <arc:FormItem Label="Password" IsRequired="True" Description="At least 8 characters.">
            <arc:PasswordField Text="{Binding Password}" />
        </arc:FormItem>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <arc:Button arc:Form.Action="Submit" IsDefault="True" Content="Create account" />
            <arc:Button arc:Form.Action="Reset" Variant="Secondary" Content="Reset" />
        </StackPanel>
    </StackPanel>
</arc:Form>
```

### Rules in the view model

The view model decides what is valid; the form decides when and where the errors show. Errors reach the items through
Avalonia's data validation (`DataValidationErrors`), so the form never reads the view model and stays safe for AOT and
trimming. The demos on this page implement `INotifyDataErrorInfo` by hand; with CommunityToolkit.Mvvm the same view
model looks like this:

```csharp
public partial class SignUpViewModel : ObservableValidator
{
    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter your email.")]
    [EmailAddress(ErrorMessage = "Enter an email like name@example.com.")]
    private string _email = "";

    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Enter a password.")]
    [MinLength(8, ErrorMessage = "Use at least 8 characters.")]
    private string _password = "";

    [RelayCommand]
    private void Validate() => ValidateAllProperties();        // Form.ValidateCommand

    [RelayCommand]
    private async Task CreateAccountAsync()                    // Form.SubmitCommand
    {
        ValidateAllProperties();                               // the form only sees mounted fields
        if (HasErrors)
        {
            return;
        }
        await _api.CreateAsync(Email, Password);
    }
}
```

An item shows the first error and keeps all of them in `Errors`. Several rules on one property report in order, so the
first rule that fails is the one shown. A value the binding cannot convert (`abc` in a number bound to an `int`) is
collected too and blocks the submit, although the view model never sees it.

Do not turn on Avalonia's DataAnnotations plugin (`WithDataAnnotationsValidation()`) together with `ObservableValidator`:
both report the same rules and every error shows twice. Trimmed and AOT apps are better off with hand-written rules,
because .NET's DataAnnotations `Validator` needs reflection.

### Submitting

`SubmitAsync()` (or a button with `arc:Form.Action="Submit"`; with `IsDefault="True"` Enter submits) does this:

1. Shows every item's errors and runs `ValidateCommand`, so fields nobody has touched are checked too.
2. Waits while any item `IsValidating`. A second submit meanwhile returns the same task; `ResetFields()` or unloading
   the form stops the wait.
3. With errors: raises `SubmitFailed` with the invalid items in document order, scrolls the first one into view with its
   label and focuses its field, and announces how many fields need attention. Set `SuppressFocus` in the handler to
   move focus yourself, for example after switching tabs.
4. Without errors: raises `Submitted`, then runs `SubmitCommand` with `SubmitCommandParameter`.

The Preview's submit button sits below the fold: submit an empty form and it scrolls back to the first error.

| Method | Does |
|---|---|
| `Validate()` | Shows every error, runs `ValidateCommand`, returns whether the form is valid (false while a check runs) |
| `ValidateAsync()` | The same, after waiting for running checks |
| `SubmitAsync()` | The submit above; true when the form was valid and the command ran |
| `ResetFields()` | Runs `ResetCommand`, then `ClearValidation()`, then raises `FieldsReset` |
| `ClearValidation()` | Hides the errors shown and forgets the submit; the view model's errors stay (the issue's `ClearValidate()`) |
| `FocusFirstError()` | Scrolls to and focuses the first invalid item, for example after server errors arrived |
| `GetInvalidItems()` | The invalid items in document order, for your own summary |

`HasErrors`, `ErrorCount`, `IsValidating` and `IsSubmitAttempted` are read-only properties you can bind.

### When errors show

`ErrorTrigger` decides when an item starts to show the errors it has collected. It does not run any rule: when the view
model checks a value is up to the binding (`UpdateSourceTrigger=LostFocus` checks only when the field is left).

| `ErrorTrigger` | Errors show |
|---|---|
| `Change` | As soon as they arrive, like a standalone field |
| `Blur` (default) | Once focus leaves the item (opening its dropdown does not count), or on submit |
| `Submit` | Only on submit or `Validate()` |

Once an item shows its errors, fixing the value hides the error at once and new errors show as they arrive. The trigger
is an inherited attached property: set it on the form, on a section panel or on one item.

<Demo name="ErrorTrigger" />

### Asynchronous checks

The view model debounces, calls the server and reports through `ErrorsChanged`; it also keeps a flag that the item binds
to `IsValidating`. While it is true the item shows "Checking…" instead of its old error, and a submit waits for it.
Clear the flag in a `finally` block: the form waits without a timeout.

<Demo name="AsyncCheck" />

### Rules across fields

A rule that compares two fields (a password confirmation) lives in the view model: when the password changes, check the
confirmation again. The error shows on the item that reports it; with `Blur`, once the confirmation has been left. The
Preview does this.

### Where errors show

`ErrorPlacement`: `Inline` (default) opens a row under the field; `Tooltip` takes no space and shows the error under the
field on hover and while the item has keyboard focus, for dense forms; `None` shows only the invalid state and leaves the
text to you (`FormItem.Errors`, `GetInvalidItems()`). `ErrorTemplate` presents the error object your way, inline and in
the tooltip. An explicit `FormItem.ErrorMessage`, such as a server error, shows at once and wins over the binding errors.

<Demo name="ErrorPlacement" />

### Label placement

`LabelPlacement="Left"` puts labels in a column. With `LabelWidth` left at NaN the column takes the widest label of the
form (at most 240 px; longer labels wrap); set `Grid.IsSharedSizeScope="True"` on a section to align it on its own, or set
`LabelWidth` to a fixed width. The label is centred on a field of the item's `Size` (give a small field a
`Size="Small"` item).

<Demo name="LabelPlacement" />

`RequiredIndicator` marks required items with an asterisk (default), marks the others "Optional" instead, or shows no
mark. `IsRequired` adds no rule.

### Items that come and go

Items in an `ItemsControl` template join the form when their row is added and leave it when the row is removed. Each row
keeps its own rules; the parent view model checks the rows on submit.

<Demo name="DynamicItems" />

The form only counts the items that are mounted: fields on a tab that is not selected, rows a virtualizing list has not
built and closed popups are not in it. Keep checking `HasErrors` in the view model's submit command, or set
`ErrorSource` to the view model: its `HasErrors` then blocks the submit (with `SubmitFailed.FirstInvalidItem` null) and
its entity-level errors (`GetErrors(null)`) are in `FormErrors` for an alert of your own. Some implementations, such as
`ObservableValidator`, return every error for `GetErrors(null)`.

### Read-only and switching validation off

`IsReadOnly` (inherited, so it also works on a section) makes the Arc text fields, `Cascader`, `TreeSelect`, `Rating`,
`TokenTextEditor` and `RichTextEditor` read-only and hides the required marks. Checkboxes, switches, selects, pickers and
sliders have no read-only mode yet and stay editable: disable them, or the whole form with `IsEnabled="False"`.
`IsValidationEnabled="False"` on the form or an item hides its errors, stops counting them and lets a submit through;
switch the matching rules off in the view model as well. Disabled, hidden and read-only items are skipped
automatically.

<Demo name="ReadOnly" />

### Fields inside an item

Arc fields inside an item leave their label and error row to it and turn red only while the item shows its errors; their
own description row stays. The value properties below report their binding's validation errors, so they work in a form:

| Control | Validated properties |
|---|---|
| `Input`, `Textarea`, `PasswordField`, `SearchField` | `Text` |
| `NumberField`, `MoneyInput`, `PhoneInput` | `Value` (and `Text`) |
| `DatePicker`, `TimePicker`, `DateRangePicker`, `ColorPicker` | `SelectedDate`, `SelectedTime`, `SelectedRange`, `Color` |
| `Checkbox`, `Switch` | `IsChecked` |
| `Select`, `Combobox`, `MorphSelect`, `MultiSelect`, `RadioGroup`, `RadioCards`, `SegmentedControl`, `ChipGroup`, `ToggleGroup` | `SelectedItem`, `SelectedValue`, `SelectedIndex` (`SelectedItems` on `MultiSelect` and `ChipGroup`) |
| `Cascader`, `TreeSelect` | `SelectedItem`, `SelectedValue` (`SelectedValues` on `TreeSelect`) |
| `TagInput`, `Slider`, `Rating`, `ScrubInput`, `ShortcutRecorder`, `EnumSelector` | `Value` (`LowerValue` and `UpperValue` on a range `Slider`), `SelectedEnum` |

A control reports one set of errors: when two of its validated properties are bound, the last report wins. Controls of
other libraries keep their own error display (the Fluent theme's `DataValidationErrors`), which then shows next to the
item's row; turn it off with a style.

## API reference

<!-- api: Form -->

<!-- api: FormItem -->

<!-- api: FormSubmitFailedEventArgs -->

<!-- api: FormLabelPlacement -->

<!-- api: FormErrorTrigger -->

<!-- api: FormErrorPlacement -->

<!-- api: FormRequiredIndicator -->

<!-- api: FormAction -->

## Accessibility

- The form is a form landmark; give it a name with `AutomationProperties.Name`.
- An item labels its field (`Target`, or the first focusable editor): the label is `LabeledBy`, the label text is the
  name unless you set one, `IsRequired` is `IsRequiredForForm`, and the help text holds the description and the error
  once it is shown. An error that is not shown yet is not read out either.
- A new error is announced assertively; with `Tooltip` or `None` placement a hidden live text says the same. A submit or
  `Validate()` that shows many errors announces one summary instead ("2 fields need attention"), then focus moves to the
  first invalid field, which reads its name and error.
- "Checking…" is announced politely. The asterisk and "Optional" are decoration; the texts follow the app language
  (en-US, zh-CN, zh-TW).

### Keyboard

| Key | Action |
|---|---|
| Tab | Moves through the fields in content order |
| Enter | Submits through a button with `arc:Form.Action="Submit"` and `IsDefault="True"` |

After a failed submit focus moves to the first invalid field as keyboard focus, so non-text controls show their focus
ring. The form does not handle any other key.

## Motion

- Error, description and "Checking…" rows open and close on the smooth spring and rewrite only the words that change,
  like a field's message rows. A field's border turns red with its usual transition; the tooltip moves like any tooltip.
- An error presented by `ErrorTemplate` appears without motion, and scrolling to the first error is immediate.
- With reduced motion rows appear at full height and words fade; with motion off everything is immediate.

## Responsive behavior

- The form takes the width it is given; items stretch to it.
- Left labels share one column and wrap past 240 px. The form does not switch them to the top on narrow screens: choose
  `LabelPlacement="Top"` for narrow layouts (for example from `ArcViewport.IsCompact`).
- Tooltip placement keeps a dense form's rows from growing when errors appear.

## Related

- `Field`: the label, description and error structure of one editor.
- `Input`, `PasswordField`, `Select`, `Checkbox`: the fields that go inside items.
- `PropertyGrid`: edit an object's properties without writing a form.
- `MultiStepForm`: a form split into steps.
