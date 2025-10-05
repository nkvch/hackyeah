import { Injectable, signal } from '@angular/core';

export type Language = 'en' | 'pl';

interface Translations {
  [key: string]: {
    en: string;
    pl: string;
  };
}

@Injectable({
  providedIn: 'root',
})
export class TranslationService {
  private currentLanguage = signal<Language>('en');

  private translations: Translations = {
    // Navigation
    'nav.profile': { en: 'Profile', pl: 'Profil' },
    'nav.messages': { en: 'Messages', pl: 'Wiadomości' },
    'nav.logout': { en: 'Logout', pl: 'Wyloguj' },
    'nav.back': { en: 'Back', pl: 'Wstecz' },
    'nav.backToMessages': { en: 'Back to Messages', pl: 'Powrót do wiadomości' },
    'nav.backToProfile': { en: 'Back to Profile', pl: 'Powrót do profilu' },

    // Messages List
    'messages.title': { en: 'My Messages', pl: 'Moje Wiadomości' },
    'messages.newMessage': { en: 'New Message', pl: 'Nowa Wiadomość' },
    'messages.loading': { en: 'Loading messages...', pl: 'Ładowanie wiadomości...' },
    'messages.retry': { en: 'Retry', pl: 'Spróbuj ponownie' },
    'messages.noMessages': { en: 'No messages yet', pl: 'Brak wiadomości' },
    'messages.noMessagesDesc': {
      en: "You don't have any messages. Start a conversation by composing a new message.",
      pl: 'Nie masz jeszcze żadnych wiadomości. Rozpocznij rozmowę, tworząc nową wiadomość.',
    },
    'messages.composeMessage': { en: 'Compose Message', pl: 'Napisz Wiadomość' },
    'messages.from': { en: 'From', pl: 'Od' },
    'messages.subject': { en: 'Subject', pl: 'Temat' },
    'messages.context': { en: 'Context', pl: 'Kontekst' },
    'messages.date': { en: 'Date', pl: 'Data' },
    'messages.previous': { en: 'Previous', pl: 'Poprzednia' },
    'messages.next': { en: 'Next', pl: 'Następna' },
    'messages.page': { en: 'Page', pl: 'Strona' },
    'messages.of': { en: 'of', pl: 'z' },
    'messages.count': { en: 'messages', pl: 'wiadomości' },

    // Message Detail
    'message.loadingMessage': { en: 'Loading message...', pl: 'Ładowanie wiadomości...' },
    'message.goBack': { en: 'Go Back', pl: 'Wróć' },
    'message.attachments': { en: 'Attachments', pl: 'Załączniki' },
    'message.download': { en: 'Download', pl: 'Pobierz' },
    'message.reply': { en: 'Reply', pl: 'Odpowiedz' },
    'message.cancelReply': { en: 'Cancel Reply', pl: 'Anuluj odpowiedź' },
    'message.yourReply': { en: 'Your Reply', pl: 'Twoja odpowiedź' },
    'message.messageLabel': { en: 'Message', pl: 'Wiadomość' },
    'message.typePlaceholder': { en: 'Type your reply...', pl: 'Wpisz swoją odpowiedź...' },
    'message.attachmentsOptional': { en: 'Attachments (optional)', pl: 'Załączniki (opcjonalne)' },
    'message.sendReply': { en: 'Send Reply', pl: 'Wyślij odpowiedź' },
    'message.sending': { en: 'Sending...', pl: 'Wysyłanie...' },
    'message.cancel': { en: 'Cancel', pl: 'Anuluj' },

    // Compose Message
    'compose.title': { en: 'Compose Message', pl: 'Nowa Wiadomość' },
    'compose.description': {
      en: 'Send a message with optional file attachments',
      pl: 'Wyślij wiadomość z opcjonalnymi załącznikami',
    },

    // Profile
    'profile.title': { en: 'My Profile', pl: 'Mój Profil' },
    'profile.accessRequest': { en: 'Access Request', pl: 'Wniosek o dostęp' },
    'profile.viewMessages': { en: 'View Messages', pl: 'Zobacz wiadomości' },
    'profile.composeMessage': { en: 'Compose Message', pl: 'Napisz wiadomość' },
    'profile.logout': { en: 'Logout', pl: 'Wyloguj' },
    'profile.editProfile': { en: 'Edit Profile', pl: 'Edytuj profil' },
    'profile.loading': { en: 'Loading...', pl: 'Ładowanie...' },
    'profile.personalInfo': { en: 'Personal Information', pl: 'Informacje osobiste' },
    'profile.accountInfo': { en: 'Account Information', pl: 'Informacje o koncie' },
    'profile.organizationInfo': { en: 'Organization Information', pl: 'Informacje o organizacji' },
    'profile.firstName': { en: 'First Name', pl: 'Imię' },
    'profile.lastName': { en: 'Last Name', pl: 'Nazwisko' },
    'profile.email': { en: 'Email', pl: 'E-mail' },
    'profile.phoneNumber': { en: 'Phone Number', pl: 'Numer telefonu' },
    'profile.pesel': { en: 'PESEL', pl: 'PESEL' },
    'profile.accountType': { en: 'Account Type', pl: 'Typ konta' },
    'profile.accountCreated': { en: 'Account Created', pl: 'Konto utworzone' },
    'profile.lastLogin': { en: 'Last Login', pl: 'Ostatnie logowanie' },
    'profile.protected': { en: 'PROTECTED', pl: 'CHRONIONE' },
    'profile.peselCannotChange': {
      en: 'PESEL cannot be changed',
      pl: 'PESEL nie może zostać zmieniony',
    },
    'profile.organizationType': { en: 'Organization Type', pl: 'Typ organizacji' },
    'profile.organizationName': { en: 'Organization Name', pl: 'Nazwa organizacji' },
    'profile.nip': { en: 'Tax ID (NIP)', pl: 'NIP' },
    'profile.regon': { en: 'REGON', pl: 'REGON' },
    'profile.krs': { en: 'KRS', pl: 'KRS' },
    'profile.address': { en: 'Address', pl: 'Adres' },
    'profile.city': { en: 'City', pl: 'Miasto' },
    'profile.postalCode': { en: 'Postal Code', pl: 'Kod pocztowy' },
    'profile.save': { en: 'Save Changes', pl: 'Zapisz zmiany' },
    'profile.cancel': { en: 'Cancel', pl: 'Anuluj' },
    'profile.saving': { en: 'Saving...', pl: 'Zapisywanie...' },
    'profile.optional': { en: 'Optional', pl: 'Opcjonalne' },

    // Login
    'login.title': { en: 'Sign in to your account', pl: 'Zaloguj się do konta' },
    'login.subtitle': { en: 'Enter your credentials to continue', pl: 'Wprowadź dane logowania' },
    'login.email': { en: 'Email', pl: 'E-mail' },
    'login.emailPlaceholder': { en: 'Enter your email', pl: 'Wprowadź adres e-mail' },
    'login.password': { en: 'Password', pl: 'Hasło' },
    'login.passwordPlaceholder': { en: 'Enter your password', pl: 'Wprowadź hasło' },
    'login.loginButton': { en: 'Sign In', pl: 'Zaloguj się' },
    'login.loggingIn': { en: 'Signing in...', pl: 'Logowanie...' },
    'login.noAccount': { en: "Don't have an account?", pl: 'Nie masz konta?' },
    'login.register': { en: 'Register here', pl: 'Zarejestruj się' },
    'login.needActivation': { en: 'Need to activate your account?', pl: 'Musisz aktywować konto?' },
    'login.resendActivation': {
      en: 'Resend activation email',
      pl: 'Wyślij ponownie e-mail aktywacyjny',
    },

    // Register
    'register.title': { en: 'Create Your Account', pl: 'Utwórz konto' },
    'register.subtitle': {
      en: 'Register for the UKNF Communication Platform',
      pl: 'Zarejestruj się w Platformie Komunikacyjnej UKNF',
    },
    'register.personalInfo': { en: 'Personal Information', pl: 'Informacje osobiste' },
    'register.firstName': { en: 'First Name', pl: 'Imię' },
    'register.lastName': { en: 'Last Name', pl: 'Nazwisko' },
    'register.email': { en: 'Email', pl: 'E-mail' },
    'register.phoneNumber': { en: 'Phone Number', pl: 'Numer telefonu' },
    'register.organizationInfo': { en: 'Organization Information', pl: 'Informacje o organizacji' },
    'register.organizationType': { en: 'Organization Type', pl: 'Typ organizacji' },
    'register.selectType': { en: 'Select type...', pl: 'Wybierz typ...' },
    'register.organizationName': { en: 'Organization Name', pl: 'Nazwa organizacji' },
    'register.nip': { en: 'Tax ID (NIP)', pl: 'NIP' },
    'register.address': { en: 'Address', pl: 'Adres' },
    'register.city': { en: 'City', pl: 'Miasto' },
    'register.postalCode': { en: 'Postal Code', pl: 'Kod pocztowy' },
    'register.registerButton': { en: 'Register', pl: 'Zarejestruj' },
    'register.registering': { en: 'Registering...', pl: 'Rejestracja...' },
    'register.alreadyHaveAccount': { en: 'Already have an account?', pl: 'Masz już konto?' },
    'register.login': { en: 'Log in', pl: 'Zaloguj się' },

    // Activate
    'activate.title': { en: 'Account Activation', pl: 'Aktywacja konta' },
    'activate.activating': { en: 'Activating your account...', pl: 'Aktywacja konta...' },
    'activate.pleaseWait': {
      en: 'Please wait while we verify your activation link...',
      pl: 'Proszę czekać, weryfikujemy link aktywacyjny...',
    },
    'activate.success': {
      en: 'Account activated successfully!',
      pl: 'Konto zostało pomyślnie aktywowane!',
    },
    'activate.accountActivated': { en: 'Account Activated!', pl: 'Konto aktywowane!' },
    'activate.setPassword': {
      en: 'Please set your password to complete the registration.',
      pl: 'Ustaw hasło, aby dokończyć rejestrację.',
    },
    'activate.goToSetPassword': { en: 'Set Password', pl: 'Ustaw hasło' },
    'activate.goToLogin': { en: 'Go to Login', pl: 'Przejdź do logowania' },
    'activate.requestNewLink': { en: 'Request New Link', pl: 'Wyślij nowy link' },
    'activate.invalidLink': { en: 'Invalid Activation Link', pl: 'Nieprawidłowy link aktywacyjny' },
    'activate.expiredLink': { en: 'Link Expired', pl: 'Link wygasł' },
    'activate.alreadyActivated': { en: 'Already Activated', pl: 'Już aktywowane' },

    // Resend Activation
    'resendActivation.title': {
      en: 'Resend Activation Email',
      pl: 'Wyślij ponownie e-mail aktywacyjny',
    },
    'resendActivation.subtitle': {
      en: 'Enter your email to receive a new activation link',
      pl: 'Wprowadź adres e-mail, aby otrzymać nowy link aktywacyjny',
    },
    'resendActivation.email': { en: 'Email', pl: 'E-mail' },
    'resendActivation.emailPlaceholder': {
      en: 'Enter your registered email',
      pl: 'Wprowadź zarejestrowany adres e-mail',
    },
    'resendActivation.sendButton': { en: 'Send Activation Email', pl: 'Wyślij e-mail aktywacyjny' },
    'resendActivation.sending': { en: 'Sending...', pl: 'Wysyłanie...' },
    'resendActivation.backToLogin': { en: 'Back to Login', pl: 'Powrót do logowania' },

    // Set Password
    'setPassword.title': { en: 'Set Your Password', pl: 'Ustaw hasło' },
    'setPassword.subtitle': {
      en: 'Create a secure password for your account',
      pl: 'Utwórz bezpieczne hasło dla swojego konta',
    },
    'setPassword.newPassword': { en: 'New Password', pl: 'Nowe hasło' },
    'setPassword.confirmPassword': { en: 'Confirm Password', pl: 'Potwierdź hasło' },
    'setPassword.requirements': { en: 'Password Requirements:', pl: 'Wymagania dotyczące hasła:' },
    'setPassword.minLength': { en: 'At least 8 characters', pl: 'Co najmniej 8 znaków' },
    'setPassword.uppercase': { en: 'One uppercase letter', pl: 'Jedna wielka litera' },
    'setPassword.lowercase': { en: 'One lowercase letter', pl: 'Jedna mała litera' },
    'setPassword.number': { en: 'One number', pl: 'Jedna cyfra' },
    'setPassword.specialChar': { en: 'One special character', pl: 'Jeden znak specjalny' },
    'setPassword.setButton': { en: 'Set Password', pl: 'Ustaw hasło' },
    'setPassword.setting': { en: 'Setting password...', pl: 'Ustawianie hasła...' },

    // Access Request
    'accessRequest.title': { en: 'My Access Request', pl: 'Mój wniosek o dostęp' },
    'accessRequest.subtitle': {
      en: 'Request access to entities for regulatory reporting',
      pl: 'Wniosek o dostęp do podmiotów w celu raportowania regulacyjnego',
    },
    'accessRequest.loading': { en: 'Loading...', pl: 'Ładowanie...' },
    'accessRequest.noRequest': {
      en: 'No access request found',
      pl: 'Nie znaleziono wniosku o dostęp',
    },
    'accessRequest.notSubmitted': {
      en: 'You have not submitted an access request yet.',
      pl: 'Nie złożyłeś jeszcze wniosku o dostęp.',
    },
    'accessRequest.createRequest': { en: 'Create Access Request', pl: 'Utwórz wniosek o dostęp' },
    'accessRequest.status': { en: 'Status', pl: 'Status' },
    'accessRequest.submittedOn': { en: 'Submitted on', pl: 'Złożono' },
    'accessRequest.requestedEntities': { en: 'Requested Entities', pl: 'Wnioskowane podmioty' },
    'accessRequest.entity': { en: 'Entity', pl: 'Podmiot' },
    'accessRequest.type': { en: 'Type', pl: 'Typ' },

    // Submit Report
    'report.title': { en: 'Submit Report', pl: 'Złóż raport' },
    'report.subtitle': {
      en: 'Upload regulatory report for your entities',
      pl: 'Prześlij raport regulacyjny dla swoich podmiotów',
    },
    'report.selectEntity': { en: 'Select Entity', pl: 'Wybierz podmiot' },
    'report.selectEntityPlaceholder': { en: 'Choose an entity...', pl: 'Wybierz podmiot...' },
    'report.reportType': { en: 'Report Type', pl: 'Typ raportu' },
    'report.selectTypePlaceholder': { en: 'Select report type...', pl: 'Wybierz typ raportu...' },
    'report.description': { en: 'Description', pl: 'Opis' },
    'report.descriptionPlaceholder': {
      en: 'Enter report description (optional)',
      pl: 'Wprowadź opis raportu (opcjonalnie)',
    },
    'report.file': { en: 'Report File', pl: 'Plik raportu' },
    'report.fileRequirement': {
      en: 'Required: Excel file (.xlsx), max 100 MB',
      pl: 'Wymagane: plik Excel (.xlsx), maks. 100 MB',
    },
    'report.chooseFile': { en: 'Choose File', pl: 'Wybierz plik' },
    'report.noFileChosen': { en: 'No file chosen', pl: 'Nie wybrano pliku' },
    'report.uploadProgress': { en: 'Upload Progress', pl: 'Postęp przesyłania' },
    'report.submit': { en: 'Submit Report', pl: 'Złóż raport' },
    'report.submitting': { en: 'Submitting...', pl: 'Przesyłanie...' },
    'report.backToProfile': { en: 'Back to Profile', pl: 'Powrót do profilu' },

    // Common
    'common.loading': { en: 'Loading...', pl: 'Ładowanie...' },
    'common.error': { en: 'Error', pl: 'Błąd' },
    'common.success': { en: 'Success', pl: 'Sukces' },
    'common.save': { en: 'Save', pl: 'Zapisz' },
    'common.cancel': { en: 'Cancel', pl: 'Anuluj' },
    'common.submit': { en: 'Submit', pl: 'Wyślij' },
    'common.close': { en: 'Close', pl: 'Zamknij' },
    'common.back': { en: 'Back', pl: 'Wstecz' },
    'common.next': { en: 'Next', pl: 'Dalej' },
    'common.previous': { en: 'Previous', pl: 'Poprzedni' },
    'common.required': { en: 'Required', pl: 'Wymagane' },
    'common.optional': { en: 'Optional', pl: 'Opcjonalne' },
  };

  constructor() {
    // Load saved language preference from localStorage
    const savedLang = localStorage.getItem('language') as Language;
    if (savedLang && (savedLang === 'en' || savedLang === 'pl')) {
      this.currentLanguage.set(savedLang);
    }
  }

  getCurrentLanguage(): Language {
    return this.currentLanguage();
  }

  setLanguage(lang: Language): void {
    this.currentLanguage.set(lang);
    localStorage.setItem('language', lang);
  }

  translate(key: string): string {
    const translation = this.translations[key];
    if (!translation) {
      console.warn(`Translation missing for key: ${key}`);
      return key;
    }
    return translation[this.currentLanguage()];
  }

  // Helper method for components to get reactive language signal
  get language() {
    return this.currentLanguage.asReadonly();
  }
}
