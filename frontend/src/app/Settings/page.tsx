'use client';

import LogoutButton from './Buttons/LogoutButton';
import ChangeButton from './Buttons/ChangeButton';

export default function Settings() {
  return (
    <div className='flex flex-col justify-center items-center gap-4'>
      <h1 className='text-2xl font-semibold'>Opcje konta</h1>
      <ChangeButton url='/Settings/EmailChange' text='Zmien Email' />
      <ChangeButton url='/Settings/PasswordChange' text='Zmien hasło' />
      <LogoutButton />
    </div>
  );
}
