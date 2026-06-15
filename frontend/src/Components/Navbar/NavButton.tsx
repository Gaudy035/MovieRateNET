import { cookies } from 'next/headers';

import LoginBtn from './subcomponents/LoginBtn';
import SettingsBtn from './subcomponents/SettingsBtn';

export default async function NavButton() {
  const cookieStorage = await cookies();
  const token = cookieStorage.get('access_token');

  return <>{token ? <SettingsBtn /> : <LoginBtn />}</>;
}
