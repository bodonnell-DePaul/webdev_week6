import Publisher from "./Publisher";

interface Book {
  id?: number;
  title: string;
  author: string;
  year: number;
  genre: string;
  isAvailable: boolean;
  publisherId: number;
  publisher: Publisher;
  audioBookAvailable: boolean;
}

export default Book;