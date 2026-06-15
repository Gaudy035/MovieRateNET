interface Review {
  review_id: number;
  username: string;
  title?: string;
  body?: string;
  rating: number;
  created_at: Date;
}

export default Review;
