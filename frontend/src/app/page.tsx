import MovieBlock from '../Components/MovieBlock/MovieBlock';
import { apiFetch } from '@/lib/api';
import { cookies } from 'next/headers';
import Movie from '../Interfaces/Movie';

export default async function Home({
  searchParams,
}: {
  searchParams: Promise<{ title?: string }>;
}) {
  const { title } = await searchParams;
  let movies: Movie[] = [];

  const endpoint = title ? `/movies?title=${title}` : '/movies';

  try {
    movies = await apiFetch(endpoint, {
      method: 'GET',
      cache: 'no-store',
    });
  } catch (error) {
    console.log('Failed to get movies', error);
  }

  const cookieStorage = await cookies();
  const isLoggedIn = cookieStorage.has('access_token');

  return (
    <div className='flex flex-col justify-center items-center'>
      <div className='flex justify-start items-start max-w-4xl w-full'>
        <h1 className='text-purple-900 font-bold text-2xl py-6'>
          Popularne filmy:
        </h1>
      </div>
      <div className='flex flex-col gap-6'>
        {movies.map((movie) => (
          <MovieBlock
            key={movie.movie_id}
            movieId={movie.movie_id}
            movieTitle={movie.title}
            movieDesc={movie.description}
            imgUrl={movie.poster_url}
            averageRating={movie.average_rating}
            isLoggedIn={isLoggedIn}
          />
        ))}
      </div>
    </div>
  );
}
