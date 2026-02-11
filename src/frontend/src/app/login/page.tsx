'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { apiFetch } from '@/lib/api';

const users = [
    { role: 'Admin', email: 'admin@clinic.com', password: 'password' },
    { role: 'User', email: 'user@clinic.com', password: 'password' },
    { role: 'Viewer', email: 'viewer@clinic.com', password: 'password' },
];

export default function LoginPage() {
    const [loading, setLoading] = useState(false);
    const router = useRouter();

    const handleLogin = async (email: string) => {
        setLoading(true);
        try {
            const res = await apiFetch('/api/auth/login', {
                method: 'POST',
                body: JSON.stringify({ email, password: 'password' }),
            });

            if (res.ok) {
                const data = await res.json();
                localStorage.setItem('clinic_token', data.token);
                localStorage.setItem('clinic_user', JSON.stringify(data.user));
                router.push('/patients');
            } else {
                alert('Login failed');
            }
        } catch (err) {
            alert('Network error');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{
            minHeight: '100vh',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            background: 'linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%)',
            padding: '2rem'
        }}>
            <div className="card" style={{
                maxWidth: '440px',
                width: '100%',
                padding: '3rem 2.5rem',
                boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.25)',
                border: 'none'
            }}>
                <div style={{ textAlign: 'center', marginBottom: '2.5rem' }}>
                    <div style={{
                        background: 'var(--primary)',
                        width: '48px',
                        height: '48px',
                        borderRadius: '12px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: '1.5rem',
                        color: 'white',
                        margin: '0 auto 1rem',
                        boxShadow: '0 10px 15px -3px rgba(79, 70, 229, 0.3)'
                    }}>✙</div>
                    <h1 style={{ fontSize: '1.875rem', fontWeight: 800, color: 'var(--text-main)', letterSpacing: '-0.025em' }}>Welcome back</h1>
                    <p style={{ color: 'var(--text-muted)', marginTop: '0.5rem' }}>Select a profile to access the platform</p>
                </div>

                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                    {users.map((u) => (
                        <button
                            key={u.role}
                            onClick={() => handleLogin(u.email)}
                            disabled={loading}
                            className="btn btn-outline"
                            style={{
                                padding: '1rem',
                                justifyContent: 'flex-start',
                                border: '1px solid var(--border)',
                                background: 'white',
                                transition: 'all 0.2s ease',
                                height: 'auto'
                            }}
                            onMouseOver={(e) => {
                                e.currentTarget.style.borderColor = 'var(--primary)';
                                e.currentTarget.style.background = 'var(--primary-light)';
                            }}
                            onMouseOut={(e) => {
                                e.currentTarget.style.borderColor = 'var(--border)';
                                e.currentTarget.style.background = 'white';
                            }}
                        >
                            <div style={{
                                width: '40px',
                                height: '40px',
                                background: '#f1f5f9',
                                borderRadius: '50%',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                fontWeight: 700,
                                color: 'var(--primary)'
                            }}>
                                {u.role.charAt(0)}
                            </div>
                            <div style={{ textAlign: 'left' }}>
                                <div style={{ fontWeight: 600, fontSize: '0.925rem' }}>Login as {u.role}</div>
                                <div style={{ fontSize: '0.750rem', color: 'var(--text-muted)' }}>{u.email}</div>
                            </div>
                        </button>
                    ))}
                </div>

                <div style={{ marginTop: '2.5rem', textAlign: 'center', fontSize: '0.875rem', color: 'var(--text-muted)' }}>
                    Admin demo panel • v1.0.0
                </div>
            </div>
        </div>
    );
}
