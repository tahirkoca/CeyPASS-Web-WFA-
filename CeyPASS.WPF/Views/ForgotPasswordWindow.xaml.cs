using System.Windows;
using CeyPASS.Business.Abstractions;

namespace CeyPASS.WPF.Views;

public partial class ForgotPasswordWindow : Window
{
    private readonly string _kullaniciAdi;
    private readonly ISifreService _sifreService;
    private readonly IEmailService _emailService;
    private bool _baslatildi;

    public ForgotPasswordWindow(string kullaniciAdi, ISifreService sifreService, IEmailService emailService)
    {
        InitializeComponent();
        _kullaniciAdi = kullaniciAdi;
        _sifreService = sifreService;
        _emailService = emailService;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_baslatildi) return;
        _baslatildi = true;

        try
        {
            var sonuc = _sifreService.SifreSifirlamaBaslat(_kullaniciAdi);
            if (!sonuc.Basarili)
            {
                UiDialog.Error(sonuc.HataMesaji, "Hata", this);
                DialogResult = false;
                Close();
                return;
            }

            var masked = _emailService.MaskEmail(sonuc.Email);
            LblInfo.Text = $"Doğrulama kodu {masked} adresine gönderildi.";
            UiDialog.Info($"Doğrulama kodu {masked} adresine gönderildi.", "Bilgi", this);
            TxtKod.Focus();
        }
        catch (Exception ex)
        {
            UiDialog.Error(
                $"Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.\n{ex.Message}",
                "Hata", this);
            DialogResult = false;
            Close();
        }
    }

    private void BtnKaydet_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            FieldValidation.SetError(TxtKod, null);
            FieldValidation.SetError(PwdYeni, null);
            FieldValidation.SetError(PwdYeniTekrar, null);
            LblError.Visibility = Visibility.Collapsed;
            LblError.Text = "";

            bool hasFieldError = false;
            if (string.IsNullOrWhiteSpace(TxtKod.Text))
            {
                FieldValidation.SetError(TxtKod, "Doğrulama kodu zorunludur.");
                hasFieldError = true;
            }
            if (string.IsNullOrWhiteSpace(PwdYeni.Password))
            {
                FieldValidation.SetError(PwdYeni, "Yeni şifre zorunludur.");
                hasFieldError = true;
            }
            if (string.IsNullOrWhiteSpace(PwdYeniTekrar.Password))
            {
                FieldValidation.SetError(PwdYeniTekrar, "Şifre tekrarı zorunludur.");
                hasFieldError = true;
            }
            if (hasFieldError)
            {
                LblError.Text = "Lütfen zorunlu alanları doldurun.";
                LblError.Visibility = Visibility.Visible;
                return;
            }

            var sonuc = _sifreService.SifreSifirlamaTamamla(
                _kullaniciAdi,
                TxtKod.Text.Trim(),
                PwdYeni.Password,
                PwdYeniTekrar.Password);

            if (!sonuc.Basarili)
            {
                LblError.Text = sonuc.HataMesaji;
                LblError.Visibility = Visibility.Visible;
                return;
            }

            UiDialog.Success("Şifreniz başarıyla güncellendi.", "Başarılı", this);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            UiDialog.Error($"Beklenmeyen bir hata oluştu.\n{ex.Message}", "Hata", this);
        }
    }

    private void BtnVazgec_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Window_OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
        {
            DialogResult = false;
            Close();
            e.Handled = true;
        }
    }

    private void Window_OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            DragMove();
    }
}
