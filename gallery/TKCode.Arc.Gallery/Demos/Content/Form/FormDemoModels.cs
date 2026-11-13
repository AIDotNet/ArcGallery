using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TKCode.Arc.Gallery.Demos;

/// <summary>
/// A hand-written <see cref="INotifyDataErrorInfo"/> base for the Form demos: each property's rules run when it changes
/// and the errors keep the order of the rules. No validation package and no reflection (the gallery is trimmed); an app
/// can use CommunityToolkit.Mvvm's <c>ObservableValidator</c> instead, the form works the same.
/// </summary>
public abstract class FormDemoModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private readonly Dictionary<string, IReadOnlyList<string>> _errors = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public bool HasErrors => _errors.Values.Any(errors => errors.Count > 0);

    public IEnumerable GetErrors(string? propertyName)
        => propertyName is not null && _errors.TryGetValue(propertyName, out var errors) ? errors : Array.Empty<string>();

    /// <summary>The properties that have rules, for <see cref="ValidateAll"/>.</summary>
    protected abstract IEnumerable<string> ValidatedProperties { get; }

    /// <summary>The errors of <paramref name="property"/> right now, in rule order (empty when it is valid).</summary>
    protected abstract IEnumerable<string> Check(string property);

    /// <summary>Runs every rule, including those of fields nobody has touched (the form's ValidateCommand).</summary>
    public void ValidateAll()
    {
        foreach (var property in ValidatedProperties)
        {
            Validate(property);
        }
    }

    /// <summary>Forgets every error (after a reset).</summary>
    public void ClearErrors()
    {
        foreach (var property in _errors.Keys.ToArray())
        {
            _errors.Remove(property);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
        }
    }

    protected void Validate(string property)
    {
        var errors = Check(property).ToArray();
        var old = _errors.TryGetValue(property, out var current) ? current : Array.Empty<string>();
        if (old.SequenceEqual(errors))
        {
            return;
        }
        _errors[property] = errors;
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
    }

    /// <summary>Sets a field, raises PropertyChanged and runs the property's rules.</summary>
    protected bool Set<T>(ref T field, T value, [CallerMemberName] string property = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }
        field = value;
        Raise(property);
        if (ValidatedProperties.Contains(property))
        {
            Validate(property);
        }
        return true;
    }

    protected void Raise([CallerMemberName] string property = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    protected static bool LooksLikeEmail(string value)
    {
        var at = value.IndexOf('@');
        return at > 0 && value.IndexOf('.', at) > at + 1 && !value.EndsWith('.') && !value.Contains(' ');
    }
}

/// <summary>A minimal command for the demos.</summary>
public sealed class FormDemoCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => execute();

    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>
/// The sign-up form: required fields, an email rule, a password confirmation that is checked again whenever the password
/// changes, a user name checked against a pretend server after the typing pauses, and terms that must be accepted.
/// </summary>
public sealed class FormSignUpModel : FormDemoModel
{
    private static readonly string[] s_taken = ["admin", "arc", "design", "support"];

    private string _userName = "";
    private string _email = "";
    private string _password = "";
    private string _confirmPassword = "";
    private bool? _acceptTerms = false;
    private bool _isCheckingUserName;
    private bool _userNameTaken;
    private string _status = "";
    private CancellationTokenSource? _nameCheck;

    public FormSignUpModel()
    {
        ValidateCommand = new FormDemoCommand(ValidateAll);
        CreateAccountCommand = new FormDemoCommand(CreateAccount);
        ResetCommand = new FormDemoCommand(Reset);
    }

    /// <summary>How long the pretend server takes to answer.</summary>
    public TimeSpan CheckDelay { get; set; } = TimeSpan.FromMilliseconds(700);

    public string UserName
    {
        get => _userName;
        set
        {
            if (Set(ref _userName, value))
            {
                _ = CheckUserNameAsync(value);
            }
        }
    }

    public string Email { get => _email; set => Set(ref _email, value); }

    public string Password
    {
        get => _password;
        set
        {
            if (Set(ref _password, value) && _confirmPassword.Length > 0)
            {
                // Cross-field rule: the confirmation depends on the password, so check it again.
                Validate(nameof(ConfirmPassword));
            }
        }
    }

    public string ConfirmPassword { get => _confirmPassword; set => Set(ref _confirmPassword, value); }

    public bool? AcceptTerms { get => _acceptTerms; set => Set(ref _acceptTerms, value); }

    /// <summary>True while the pretend server checks the user name (bound to FormItem.IsValidating).</summary>
    public bool IsCheckingUserName
    {
        get => _isCheckingUserName;
        private set
        {
            if (_isCheckingUserName != value)
            {
                _isCheckingUserName = value;
                Raise();
            }
        }
    }

    public string Status
    {
        get => _status;
        private set
        {
            if (_status != value)
            {
                _status = value;
                Raise();
            }
        }
    }

    public FormDemoCommand ValidateCommand { get; }

    public FormDemoCommand CreateAccountCommand { get; }

    public FormDemoCommand ResetCommand { get; }

    protected override IEnumerable<string> ValidatedProperties =>
        [nameof(UserName), nameof(Email), nameof(Password), nameof(ConfirmPassword), nameof(AcceptTerms)];

    protected override IEnumerable<string> Check(string property)
    {
        switch (property)
        {
            case nameof(UserName):
                if (UserName.Length == 0)
                {
                    yield return "Enter a user name.";
                }
                else if (UserName.Length < 3 || UserName.Length > 20 || !UserName.All(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '.'))
                {
                    yield return "Use 3 to 20 lowercase letters, digits or dots.";
                }
                else if (_userNameTaken)
                {
                    yield return "That user name is taken.";
                }
                break;
            case nameof(Email):
                if (Email.Length == 0)
                {
                    yield return "Enter your email.";
                }
                else if (!LooksLikeEmail(Email))
                {
                    yield return "Enter an email like name@example.com.";
                }
                break;
            case nameof(Password):
                if (Password.Length == 0)
                {
                    yield return "Enter a password.";
                }
                else if (Password.Length < 8)
                {
                    yield return "Use at least 8 characters.";
                }
                break;
            case nameof(ConfirmPassword):
                if (ConfirmPassword != Password)
                {
                    yield return "The passwords don't match.";
                }
                break;
            case nameof(AcceptTerms):
                if (AcceptTerms != true)
                {
                    yield return "Accept the terms to continue.";
                }
                break;
        }
    }

    private async Task CheckUserNameAsync(string name)
    {
        _nameCheck?.Cancel();
        var check = _nameCheck = new CancellationTokenSource();
        _userNameTaken = false;
        Validate(nameof(UserName));
        if (GetErrors(nameof(UserName)).Cast<string>().Any())
        {
            IsCheckingUserName = false;
            return;
        }
        IsCheckingUserName = true;
        try
        {
            await Task.Delay(CheckDelay, check.Token);
            if (_nameCheck == check)
            {
                _userNameTaken = s_taken.Contains(name);
                Validate(nameof(UserName));
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (_nameCheck == check)
            {
                IsCheckingUserName = false;
            }
        }
    }

    private void CreateAccount()
    {
        // The view model checks again: the form only sees the fields that are on screen.
        ValidateAll();
        if (HasErrors || IsCheckingUserName)
        {
            return;
        }
        Status = $"Account created for {UserName}.";
    }

    private void Reset()
    {
        _nameCheck?.Cancel();
        _userNameTaken = false;
        _userName = _email = _password = _confirmPassword = "";
        _acceptTerms = false;
        IsCheckingUserName = false;
        Raise(nameof(UserName));
        Raise(nameof(Email));
        Raise(nameof(Password));
        Raise(nameof(ConfirmPassword));
        Raise(nameof(AcceptTerms));
        Status = "";
        ClearErrors();
    }
}

/// <summary>Two fields with simple rules, used where the demo is about the form rather than the rules.</summary>
public sealed class FormProfileModel : FormDemoModel
{
    private string _name = "";
    private string _email = "";
    private string _company = "";
    private string _role = "";
    private string _phone = "";
    private object? _plan;

    public FormProfileModel()
    {
        ValidateCommand = new FormDemoCommand(ValidateAll);
    }

    public string Name { get => _name; set => Set(ref _name, value); }

    public string Email { get => _email; set => Set(ref _email, value); }

    public string Company { get => _company; set => Set(ref _company, value); }

    public string Role { get => _role; set => Set(ref _role, value); }

    public string Phone { get => _phone; set => Set(ref _phone, value); }

    public object? Plan { get => _plan; set => Set(ref _plan, value); }

    public FormDemoCommand ValidateCommand { get; }

    protected override IEnumerable<string> ValidatedProperties => [nameof(Name), nameof(Email), nameof(Company), nameof(Plan)];

    protected override IEnumerable<string> Check(string property)
    {
        switch (property)
        {
            case nameof(Name) when Name.Trim().Length == 0:
                yield return "Enter your name.";
                break;
            case nameof(Email) when Email.Length == 0:
                yield return "Enter your email.";
                break;
            case nameof(Email) when !LooksLikeEmail(Email):
                yield return "Enter an email like name@example.com.";
                break;
            case nameof(Company) when Company.Trim().Length < 2:
                yield return "Enter the company name.";
                break;
            case nameof(Plan) when Plan is null:
                yield return "Choose a plan.";
                break;
        }
    }
}

/// <summary>One backup contact row: its own rules live with the row.</summary>
public sealed class FormContactModel : FormDemoModel
{
    private string _email = "";

    public FormContactModel(int number) => Number = number;

    public int Number { get; }

    public string Label => $"Backup email {Number}";

    public string Email { get => _email; set => Set(ref _email, value); }

    protected override IEnumerable<string> ValidatedProperties => [nameof(Email)];

    protected override IEnumerable<string> Check(string property)
    {
        if (property == nameof(Email) && !LooksLikeEmail(Email))
        {
            yield return Email.Length == 0 ? "Enter an email or remove the row." : "Enter an email like name@example.com.";
        }
    }
}

/// <summary>A list of contact rows that can grow and shrink; the form sees the rows that are mounted.</summary>
public sealed class FormContactsModel
{
    private int _next = 1;

    public FormContactsModel()
    {
        Add();
        AddCommand = new FormDemoCommand(Add);
        ValidateCommand = new FormDemoCommand(() =>
        {
            foreach (var contact in Contacts)
            {
                contact.ValidateAll();
            }
        });
    }

    public ObservableCollection<FormContactModel> Contacts { get; } = [];

    public FormDemoCommand AddCommand { get; }

    public FormDemoCommand ValidateCommand { get; }

    public void Add() => Contacts.Add(new FormContactModel(_next++));

    public void Remove(FormContactModel contact) => Contacts.Remove(contact);
}
