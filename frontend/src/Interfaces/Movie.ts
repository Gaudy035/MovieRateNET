interface Movie {
  movie_id: number;
  title: string;
  description: string;
  poster_url: string;
  release_year: number;
  duration: number;
  created_at: Date;
  average_rating: number | null;
}

export default Movie;
