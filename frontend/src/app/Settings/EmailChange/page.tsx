'use client';
import { apiFetch } from '@/lib/api';
import InputTemp from '@/Components/Forms/InputTemp';
import ButtonTemp from '@/Components/Forms/ButtonTemp';
import { useState } from 'react';
import { useRouter } from 'next/navigation';

export default function EmailChangePage() {
  const router = useRouter();
  const [errorMessage, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<boolean>(false);
  const handleSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setSuccess(false);

    const formData = new FormData(e.currentTarget);
    const payload = Object.fromEntries(formData.entries());

    try {
      await apiFetch('/auth/change-email', {
        method: 'PATCH',
        body: JSON.stringify(payload),
      });

      setSuccess(true);

      setTimeout(() => {
        router.push('/Login');
        router.refresh();
      }, 2000);
    } catch (error: any) {
      console.log('API error', error);
      setError(error?.message);
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className='flex flex-col gap-4 justify-center items-center'
    >
      <h1 className='font-bold text-purple-900 text-2xl'>Zmień Email</h1>
      <InputTemp
        type='email'
        name='new_email'
        placeholder='Nowy email'
        label='Nowy email'
      />
      <InputTemp
        type='password'
        name='password'
        label='Hasło'
        placeholder='Hasło'
      />
      {errorMessage ? <p className='text-red-600'>{errorMessage}</p> : null}
      {success ? <p className='text-green-600'>Zmiana adresu udana!</p> : null}
      <ButtonTemp buttonText='Zapisz' />
    </form>
  );
}
