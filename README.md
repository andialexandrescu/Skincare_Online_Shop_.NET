# Titlul proiectului:
## Terra_Skin_Alchemy_WebApp_.NET

# Descrierea platformei “Online shop” cu urmatoarele functionalitati:

- [x] Sa existe 4 tipuri de utilizatori: utilizator neinregistrat, inregistrat,
colaborator, administrator (0.5p).

- [x]  Utilizatorul colaborator poate adauga produse in magazine. Acesta va trimite
cereri de adaugare administratorului, iar acesta le poate respinge sau aproba.
Dupa aprobare produsele vor putea fi vizualizate in magazin (1.0p).

- [x] Produsele fac parte din categorii. Categoriile sunt create dinamic de catre
administrator. Dinamic insemnand ca acesta poate adauga noi categorii
direct din interfata aplicatiei. De asemenea, adminul este cel care poate
vizualiza, edita si sterge categoriile ori de cate ori este necesar (1.0p).

- [x] Un produs are titlu, descriere, poza, pret, stoc, rating (1-5 stele), review-uri
din partea utilizatorilor. Fiecare utilizator acorda un rating de la 1 la 5.
Ratingul nu este un camp obligatoriu. In final, fiecare produs are un scor,
calculat pe baza tuturor ratingurilor existente. Review-ul este un comentariu
de tip text lasat de utilizatori. Acest camp nu este obligatoriu. Restul
campurilor sunt obligatorii. Stocul reprezinta numarul de produse din baza
de date. (1.0p).

- [x] Utilizatorul colaborator poate sa editeze si sa stearga produsele adaugate de
el. Dupa editare, produsul necesita din nou aprobare din partea
administratorului (1.0p).

- [x] Utilizatorul neinregistrat va fi redirectionat sa isi faca un cont atunci cand
incearca adaugarea unui produs in cos. Atunci cand nu are cont, el poate
doar sa vizualizeze produsele si comentariile asociate acestora (0.5p).

- [x] Atunci cand un utilizator devine utilizator inregistrat poate sa plaseze
comenzi (sa adauge produse in cos) si sa lase review-uri (nota sau text), pe
care ulterior le poate edita sau sterge (1.0p). Atunci cand un utilizator adauga
un produs in cos, acesta poate selecta si cantitatea. In momentul in care se
plaseaza (fictiv) comanda, stocul produsului trebuie sa scada corespunzator,
in functie de numarul de produse achizitionate (0.5p).

- [x] Produsele pot fi cautate dupa denumire prin intermediul unui motor de
cautare. De asemenea, produsele nu trebuie cautate dupa tot numele. Ele
trebuie sa fie gasite si in cazul in care un utilizator cauta doar anumite parti
care compun denumirea (1.0p).

- [x] Rezultatele motorului de cautare pot fi sortate crescator, respectiv
descrescator, in functie de pret si numarul de stele (se vor implementa filtre
din care un utilizator poate sa aleaga) (0.5p).

- [x] Administratorul poate sterge si edita atat produse, cat si comentarii. Acesta
poate, de asemenea, sa activeze sau sa revoce drepturile utilizatorilor (1.0p).

# Implementari tehnice:

 - (1) Am convenit la o implementare comuna pentru Products, Categories si Reviews, 
 ca dupa sa avem o baza pentru a dezvolta Carts, CartProducts (care rezolva relatia many-to-many 
 dintre produse si cosuri de cumparaturi), care implica existenta atributului de Quantity in CartProducts 
 si alte chei straine impreuna cu prorietati virtuale pentru asocierea tabelelor carora le apartin acele 
 foreign keys, avand posibilitatea accesarii altor proprietati. Rezolvarea relatiei many-to-many se vede 
 in ApplicationDbContext.
 
 - (2) Am dezvoltat ideea de a avea 4 utilizatori, insemnand ca fiecare actiune publica din fiecare controller 
 a primit atributele de Authorize(Roles=””) corespunzatoare si am stabilit ca Admin-ul sa nu aiba restrictii 
 aproape deloc asupra datelor pe care le poate sterge sau edita. De retinut ca utilizatorul neinregistrat trebuie 
 numai sa isi creeze un cont, devenind implicit User si putand deveni Partner (acel utilizator care colaboreaza cu 
 magazinul si adauga produse) sau chiar Admin daca unul dintre administratori doreste acest lucru (prin intermediul 
 UsersController).
 
 - (3) Am modificat putin logica de a cunoaste exact cine a postat produse si review-uri, astfel incat folosind metoda 
 privata SetAccessRights de la nivelul fiecarui controller (ProductsController si ReviewsController), sa se afiseze butoane 
 in Views pentru stergere si editare daca produsul/ review-ul respectiv apartin utilizatorului (Partner sau User, din moment 
 ce Admin nu are nicio restrictie).
 
 - (4) Am manipulat atributul Image din clasa Product, de tip IFormFile, ce se poate observa in actiunile Create si Edit 
 din ProductsController.
 
 - (5) O data ce am avut destule inserari de produse in baza de date locala, am implementat paginarea, filtrarea rezultatelor 
 dupa pret si o medie a rating-urilor de la nivelul tuturor review-urilor asociate fiecarui produs in parte, impreuna cu un 
 motor de cautare. Legat de paginare si cum functioneaza alaturi de filtrare, daca nu e selectat niciun tip de sortare 
 crescatoare/ descrescatoare, atunci nu se va aplica niciun filtru, nefiind afectata paginarea. E specificat in cerinta ca 
 acest motor de cautare trebuie sa aiba o cautare partiala pentru atributul Name de la Product numai dupa ce a fost introdus 
 in search bar (de retinut ca ar putea fi adaugata o implementarea asemanatoare d.p.d.v. al cautarii partiale pentru Content 
 din Review). De altfel, media de Rating a review-urilor postate pentru un produs anume va fi calculat dinamic, adica o data 
 cu adaugarea fiecarui review, fiind numai o afisare in view-ul partajat din Shared, numit ProductInfo, care calculeaza media 
 in sectiunea de Overall Rating (am mentionat asta pentru ca initial ne gandisem ca Product sa aiba atributul de Rating si el 
 sa fie modificat de fiecare data o data cu crearea, editarea sau stergerea unui review, dar ar fi insemnat sa ne complicam, 
 deci am gasit o solutie destul de buna care permite o sortare dupa criteriile rating_asc si rating_desc folosind functii lambda).
 
 - (6) Am implementat sa existe acele cereri de aprobare, respingere a produsului propus sa fie vandut la magazin de catre un 
 partener al magazinului. A fost exprimata ideea asta la nivel de cod folosind un enum cu trei posibile valori: Approved, Rejected 
 si Unverified. Acest lucru inseamna ca o data ce a fost creat un produs, cererea e trimisa in metoda Create din ProductsController, 
 iar pana nu e aprobata/ respinsa de un Admin care intra in View-ul PendingAppoval (ce afiseaza numai acele produse pentru care atributul 
 RequestStatus este Unverified), produsul pur si simplu nu va fi afisat pe pagina principala a site-ului cu o conditie de afisare doar 
 pentru produsele Accepted in Index din ProductsController (ne referim la ce a fost setat ca si cale URL default in Program.cs in 
 MapControllerRoute, adica View-ul Index din Products). De sigur, ca intregul proces a insemnat ca si metoda Edit, cu toate ca produsul 
 sa zicem ca a fost aprobat prima oara sa apara in magazinul online, va modifica atributul produsului editat RequestStatus din Accepted 
 in Unverified. De aceea, apar noi metode la care doar Admin-ul are acces: PendingApproval care are un View corespunzator si Approve si 
 Reject care nu au View-uri separate (logica e continuta tot in View-ul PendingApproval).
 
 - (7) Cand se doreste sa fie adaugat un produs in cos, acel user trebuie sa fie logat, daca nu e logat nu are acces la prea multe informatii 
 de pe site, numai metodele Index, Details din ProductsController via AllowAnonymous, de aceea o data ce se apasa pe butonul Add To Cart din 
 pagina de Details a unui produs, utilizatorul e prompted sa se logheze.
