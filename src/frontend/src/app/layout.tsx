'use client';

import { useEffect, useState } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import Link from 'next/link';
import './globals.css';

export default function Layout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const [user, setUser] = useState<any>(null);

  useEffect(() => {
    const token = localStorage.getItem('clinic_token');
    const savedUser = localStorage.getItem('clinic_user');

    if (!token && pathname !== '/login') {
      router.push('/login');
    }

    if (savedUser) setUser(JSON.parse(savedUser));
  }, [pathname]);

  const handleLogout = () => {
    localStorage.removeItem('clinic_token');
    localStorage.removeItem('clinic_user');
    router.push('/login');
  };

  const navItems = [
    { label: 'Patients', href: '/patients', icon: '👤' },
    ...(user?.role === 'Admin' ? [{ label: 'Users', href: '/users', icon: '🛠️' }] : []),
  ];

  const content = pathname === '/login' ? (
    <>{children}</>
  ) : (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      <aside style={{
        width: '280px',
        background: '#1e293b',
        color: 'white',
        padding: '2rem 1.5rem',
        display: 'flex',
        flexDirection: 'column',
        boxShadow: '4px 0 10px rgba(0,0,0,0.05)',
        position: 'fixed',
        height: '100vh'
      }}>
        <div style={{ marginBottom: '2.5rem', display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <div style={{ background: 'var(--primary)', width: '32px', height: '32px', borderRadius: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '1.2rem' }}>✙</div>
          <h2 style={{ fontSize: '1.25rem', fontWeight: 700, letterSpacing: '-0.025em' }}>Clinic POS</h2>
        </div>

        <nav style={{ flex: 1 }}>
          <ul style={{ listStyle: 'none', padding: 0 }}>
            {navItems.map(item => (
              <li key={item.href} style={{ marginBottom: '0.5rem' }}>
                <Link href={item.href} style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.75rem',
                  padding: '0.75rem 1rem',
                  borderRadius: '0.5rem',
                  color: pathname.startsWith(item.href) ? 'white' : '#94a3b8',
                  textDecoration: 'none',
                  background: pathname.startsWith(item.href) ? 'rgba(255,255,255,0.1)' : 'transparent',
                  transition: 'all 0.2s ease',
                  fontWeight: 500
                }}>
                  <span>{item.icon}</span>
                  {item.label}
                </Link>
              </li>
            ))}
          </ul>
        </nav>

        <div style={{
          marginTop: 'auto',
          padding: '1.5rem',
          background: 'rgba(255,255,255,0.05)',
          borderRadius: '1rem',
          border: '1px solid rgba(255,255,255,0.1)'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '1rem' }}>
            <div style={{ width: '40px', height: '40px', background: '#334155', borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 600 }}>
              {user?.email?.charAt(0).toUpperCase() || 'U'}
            </div>
            <div style={{ overflow: 'hidden' }}>
              <p style={{ fontSize: '0.875rem', fontWeight: 600, margin: 0, whiteSpace: 'nowrap', textOverflow: 'ellipsis' }}>{user?.email || 'User'}</p>
              <p style={{ fontSize: '0.75rem', color: '#94a3b8', margin: 0 }}>{user?.role}</p>
            </div>
          </div>
          <button
            onClick={handleLogout}
            className="btn btn-outline"
            style={{ width: '100%', borderColor: 'rgba(255,255,255,0.2)', color: 'white', fontSize: '0.875rem' }}
          >
            Logout
          </button>
        </div>
      </aside>
      <main style={{ flex: 1, marginLeft: '280px', minHeight: '100vh' }}>
        <div style={{ maxWidth: '1200px', margin: '0 auto', padding: '2rem' }}>
          {children}
        </div>
      </main>
    </div>
  );

  return (
    <html lang="en">
      <head>
        <title>Clinic POS</title>
      </head>
      <body>
        {content}
      </body>
    </html>
  );
}
