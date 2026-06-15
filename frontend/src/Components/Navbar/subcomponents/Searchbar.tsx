'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';

export default function Searchbar() {
  const [searchTitle, setSearchTitle] = useState<string | null>(null);
  const router = useRouter();

  const handleSubmit = (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (searchTitle) {
      router.push(`/?title=${searchTitle}`);
    } else {
      router.push('/');
    }
  };

  return (
    <form
      className='flex justify-center items-center gap-2'
      onSubmit={handleSubmit}
    >
      <input
        type='text'
        placeholder='Wyszukaj tytul...'
        className='bg-white text-black rounded-xl px-3 py-1'
        value={searchTitle || ''}
        onChange={(e) => setSearchTitle(e.target.value)}
      />
      <button className='cursor-pointer' type='submit'>
        <svg
          xmlns='http://www.w3.org/2000/svg'
          fill='none'
          viewBox='0 0 24 24'
          strokeWidth={2}
          stroke='currentColor'
          className='size-6'
        >
          <path
            strokeLinecap='round'
            strokeLinejoin='round'
            d='m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z'
          />
        </svg>
      </button>
    </form>
  );
}
