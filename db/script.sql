-- CREATE DATABASE movie_rate_db;

CREATE TABLE t_movies(
	movie_id SERIAL PRIMARY KEY,
	title VARCHAR(255) NOT NULL,
	description TEXT NOT NULL,
	poster_url TEXT NOT NULL,
	release_year INT NOT NULL,
	duration INT NOT NULL,
	created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE t_users(
	user_id SERIAL PRIMARY KEY,
	username VARCHAR(50) NOT NULL UNIQUE,
	email VARCHAR(255) NOT NULL UNIQUE,
	password VARCHAR(255) NOT NULL,
	created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE t_reviews(
	review_id SERIAL PRIMARY KEY,
	user_id INT NOT NULL REFERENCES
		t_users(user_id) ON DELETE CASCADE,
	movie_id INT NOT NULL REFERENCES
		t_movies(movie_id) ON DELETE CASCADE,
	title VARCHAR(50) NULL,
	body TEXT NULL,
	rating INT NOT NULL CHECK (rating >= 1 AND rating <= 10),
	created_at TIMESTAMPTZ DEFAULT NOW(),

	CONSTRAINT unique_user_review UNIQUE (user_id, movie_id)
);

-- Inserts

INSERT INTO t_movies (title, description, release_year, poster_url, duration)
VALUES
	-- Sweeney Todd
	(
	'Sweeney Todd: Demoniczny Golibroda z Fleet Street',
	'Benjamin Barker, po latach wygnania, wraca do Londynu. Pragnie zemścić się na okrutnym sędzi Turpinie, który skazał go, by posiąść jego żonę. Spotyka dawną sąsiadkę, Nellie Lovett. Opowiada ona Barkerowi o tym, że kiedy on był na wygnaniu, Turpin wziął sobie cały jego dobytek i córkę jak swoją własność. Barker w zemście zmienia nazwisko na Sweeney Todd (żeby uniknąć kolejnego procesu sądowego) i w swoim dawnym mieszkaniu nad jej zakładem otwiera nowy zakład fryzjerski, w którym podcina gardła niewinnym klientom, dokonując swej zemsty i przy okazji zaopatrując panią Lovett w nadzienie do pasztecików (bo dawno nie miała klientów). Tymczasem Anthony Hope, marynarz, który pomógł golibrodzie dostać się do Londynu, zakochuje się w Johannie. Jest to córka Benjamina, którą sędzia Turpin „przygarnął”. Obiecuje jej, że ją wykradnie. Raz Turpin przyszedł do Todda, żeby on go ogolił. Kiedy jednak zemsta się dopełnia, do pokoju wchodzi Anthony i wszystko idzie na marne. Drugi raz sędzia przychodzi nocą do Todda zwabiony listem z informacją o porwaniu Johanny przez Anthonego. Todd proponuje zabieg, a niczego niepodejrzewający sędzia z ochotą się zgadza.',
	2007,
	'https://goldenglobes.com/wp-content/uploads/2023/10/2008-sweeney_todd.jpg?w=1012',
	116
	),
	-- Edward
	(
	'Edward Nożycoręki',
	'Akwizytorka firmy Avon, Peg Boggs (Dianne Wiest), nie zdoławszy wiele zarobić w swoim rejonie, udaje się do ponurego zamczyska na wzgórzu, gdzie spotyka Edwarda (Johnny Depp). Edward to android, największe dzieło niedocenianego za życia wynalazcy (Vincent Price) mieszkającego kiedyś w zamku. Nagła śmierć przerwała wynalazcy pracę, w związku z czym Edward pozostał z ostrymi jak brzytwa nożycami zamiast dłoni. Pani Boggs zabiera Edwarda do swego domu, gdzie zaczyna się on powoli przystosowywać do życia w małomiasteczkowej społeczności. Zakochuje się w Kim Boggs (Winona Ryder), która na początku go nie cierpi, ale stopniowo zaczyna odwzajemniać jego uczucie, podczas gdy mieszkańcy miasteczka uprzedzają się do niego coraz bardziej, ze względu na jego niebezpieczne dla innych upośledzenie.',
	1990,
	'https://m.media-amazon.com/images/I/61z8BGnT0kL._AC_UF894,1000_QL80_.jpg',
	103
	)
;