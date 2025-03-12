# Project Title:
## Terra_Skin_Alchemy_WebApp_.NET

## Project Summary

It is a site that works with an MSSQLLocalDB database, written using C#, LINQ, and ASP.NET, which simulates the purchase of personal care products. Users can browse detailed product information, including category, reviews and available stock, along with placing them in the cart and completing an order. Users can also have multiple roles, with store admins, customers, and partners submitting product approval requests, which will be processed by the admins. There is also implemented a search engine and the possibility of sorting products according to price and average ratings. Developed in 3 stages, team coordination being carried out via the Trello platform.

# Description of the "Online Shop" Platform with the Following Functionalities:

- [x] There are 4 types of users: unregistered user, registered user, collaborator, and administrator (0.5p).

- [x] The collaborator user can add products to stores. They send addition requests to the administrator, who can either reject or approve them. After approval, the products can be viewed in the store (1.0p).

- [x] Products belong to categories. Categories are dynamically created by the administrator. Dynamically means that the administrator can add new categories directly from the application interface. Additionally, the admin can view, edit, and delete categories whenever necessary (1.0p).

- [x] A product has a title, description, image, price, stock, rating (1-5 stars), and user reviews. Each user can give a rating from 1 to 5. The rating is not a mandatory field. Ultimately, each product has a score calculated based on all existing ratings. The review is a text comment left by users. This field is not mandatory. The rest of the fields are mandatory. The stock represents the number of products in the database (1.0p).

- [x] The collaborator user can edit and delete the products they added. After editing, the product requires approval from the administrator again (1.0p).

- [x] The unregistered user will be redirected to create an account when attempting to add a product to the cart. When they do not have an account, they can only view products and their associated comments (0.5p).

- [x] When a user becomes a registered user, they can place orders (add products to the cart) and leave reviews (rating or text), which they can later edit or delete (1.0p). When a user adds a product to the cart, they can also select the quantity. When the order is (fictitiously) placed, the product stock must decrease accordingly based on the number of products purchased (0.5p).

- [x] Products can be searched by name using a search engine. Additionally, products do not need to be searched by their full name. They should also be found if a user searches for only certain parts of the name (1.0p).

- [x] The search engine results can be sorted in ascending or descending order based on price and the number of stars (filters will be implemented for users to choose from) (0.5p).

- [x] The administrator can delete and edit both products and comments. They can also activate or revoke user rights (1.0p).

# Technical Implementations:

- (1) We agreed on a common implementation for Products, Categories, and Reviews, so that we have a foundation to develop Carts and CartProducts (which resolve the many-to-many relationship between products and shopping carts). This involves the existence of the Quantity attribute in CartProducts and other foreign keys along with virtual properties for associating the tables to which those foreign keys belong, allowing access to other properties. The resolution of the many-to-many relationship can be seen in ApplicationDbContext.

- (2) We developed the idea of having 4 types of users, meaning that each public action in each controller received the corresponding `Authorize(Roles="")` attributes, and we established that the Admin would have almost no restrictions on the data they can delete or edit. It is important to note that the unregistered user only needs to create an account, becoming implicitly a User and potentially a Partner (the user who collaborates with the store and adds products) or even an Admin if one of the administrators desires this (via the UsersController).

- (3) We slightly modified the logic to know exactly who posted products and reviews, so that using the private `SetAccessRights` method at the level of each controller (ProductsController and ReviewsController), buttons for deletion and editing are displayed in Views if the respective product/review belongs to the user (Partner or User, since Admin has no restrictions).

- (4) We manipulated the Image attribute in the Product class, of type IFormFile, which can be observed in the Create and Edit actions in ProductsController.

- (5) Once we had enough product entries in the local database, we implemented pagination, filtering results by price, and an average of ratings from all reviews associated with each product, along with a search engine. Regarding pagination and how it works alongside filtering, if no ascending/descending sorting type is selected, then no filter will be applied, and pagination will not be affected. It is specified in the requirements that this search engine must have a partial search for the Name attribute of Product only after it has been entered in the search bar (note that a similar implementation could be added for partial search for Content in Review). Additionally, the average Rating of reviews posted for a specific product will be calculated dynamically, meaning with each review added, and it is only displayed in the shared View in Shared, named ProductInfo, which calculates the average in the Overall Rating section (we mentioned this because initially we thought that Product should have a Rating attribute that would be modified each time a review is created, edited, or deleted, but that would have complicated things, so we found a good solution that allows sorting by the criteria rating_asc and rating_desc using lambda functions).

- (6) We implemented the approval and rejection requests for products proposed to be sold in the store by a partner. This idea was expressed at the code level using an enum with three possible values: Approved, Rejected, and Unverified. This means that once a product is created, the request is sent in the Create method in ProductsController, and until it is approved/rejected by an Admin who accesses the PendingApproval View (which displays only those products for which the RequestStatus attribute is Unverified), the product will simply not be displayed on the main page of the site with a display condition only for Accepted products in Index in ProductsController (we refer to what was set as the default URL path in Program.cs in MapControllerRoute, i.e., the Index View in Products). Of course, the entire process meant that the Edit method, even if the product was initially approved to appear in the online store, will modify the RequestStatus attribute of the edited product from Accepted to Unverified. Therefore, new methods are available only to the Admin: PendingApproval, which has a corresponding View, and Approve and Reject, which do not have separate Views (the logic is contained in the PendingApproval View).

- (7) When a product is to be added to the cart, the user must be logged in. If they are not logged in, they do not have access to much information on the site, only the Index and Details methods in ProductsController via AllowAnonymous. Therefore, once the Add To Cart button is clicked on the Details page of a product, the user is prompted to log in.


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
