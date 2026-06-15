import type { Metadata } from 'next';
import './globals.css';
import Navbar from '../Components/Navbar/Navbar';
import Footer from '../Components/Footer/Footer';

export const metadata: Metadata = {
  title: 'MovieRate',
  description: 'MovieRating',
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang='en' className={`h-full antialiased`}>
      <body className='min-h-screen flex flex-col bg-purple-900'>
        <Navbar />
        <main className='bg-white pb-12 flex-1 flex justify-center items-center'>
          {children}
        </main>
        <Footer />
      </body>
    </html>
  );
}
