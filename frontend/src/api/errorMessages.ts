// Error handling layer
const statusMessages: Record<number, string> = {
  0:   'Conexiune eșuată. Verifică internetul sau încearcă mai târziu.',
  1:   'A apărut o eroare neașteptată. Contactează administratorul.',
  400: 'Datele trimise sunt incorecte. Verifică formularul.',
  401: 'Trebuie să fii autentificat pentru a continua.',
  403: 'Nu ai permisiunea să faci această acțiune.',
  404: 'Resursa căutată nu există.',
  405: 'Acțiune nepermisă. Contactează administratorul.',
  409: 'Modificarea nu a putut fi salvată deoarece există un conflict.',
  422: 'Datele nu au putut fi procesate. Verifică câmpurile și încearcă din nou.',
  429: 'Prea multe încercări. Așteaptă câteva secunde și încearcă din nou.',
  500: 'Ceva a mers greșit pe server. Încearcă din nou sau contactează administratorul.',
  502: 'Serviciul este temporar indisponibil. Încearcă din nou.',
  503: 'Serviciul este în mentenanță. Revino mai târziu.',
  504: 'Răspunsul a durat prea mult. Încearcă din nou.',
};

export function getStatusMessage(status: number): string {
  return statusMessages[status] ?? statusMessages[1];
}
