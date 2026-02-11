'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';

export default function UsersPage() {
    const [users, setUsers] = useState<any[]>([]);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(true);
    const router = useRouter();

    useEffect(() => {
        const fetchUsers = async () => {
            const token = localStorage.getItem('clinic_token');
            if (!token) return;

            try {
                const res = await fetch('http://localhost:8081/api/users', {
                    headers: { 'Authorization': `Bearer ${token}` }
                });

                if (res.status === 403) {
                    setError('Access Denied: Only Admins can manage users.');
                    setLoading(false);
                    return;
                }

                if (res.ok) {
                    const data = await res.json();
                    // The endpoint returns a single user for GET {id}, but we need a list.
                    // For the thin slice demo, we normally seed/list, 
                    // but I'll make it handle the error gracefully or show the user.
                    setUsers(Array.isArray(data) ? data : [data]);
                } else {
                    setError('Failed to fetch users. Ensure you are logged in as Admin.');
                }
            } catch (err) {
                setError('Connection error.');
            } finally {
                setLoading(false);
            }
        };

        fetchUsers();
    }, []);

    if (loading) return <div className="card">Loading users...</div>;

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
                <div>
                    <h1 style={{ fontSize: '1.875rem', fontWeight: 700, color: '#0f172a', marginBottom: '0.25rem' }}>User Management</h1>
                    <p style={{ color: '#64748b' }}>Manage system access and roles (Admin Only)</p>
                </div>
            </div>

            {error ? (
                <div className="card" style={{ borderColor: '#fca5a5', background: '#fef2f2' }}>
                    <p style={{ color: '#dc2626', fontWeight: 600 }}>{error}</p>
                    <button onClick={() => router.push('/patients')} className="btn btn-primary" style={{ marginTop: '1rem' }}>
                        Back to Patients
                    </button>
                </div>
            ) : (
                <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
                    <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                        <thead>
                            <tr style={{ background: '#f8fafc', borderBottom: '1px solid #e2e8f0' }}>
                                <th style={{ padding: '1rem 1.5rem', textAlign: 'left', fontSize: '0.75rem', fontWeight: 600, color: '#64748b', textTransform: 'uppercase' }}>Email</th>
                                <th style={{ padding: '1rem 1.5rem', textAlign: 'left', fontSize: '0.75rem', fontWeight: 600, color: '#64748b', textTransform: 'uppercase' }}>Role</th>
                                <th style={{ padding: '1rem 1.5rem', textAlign: 'left', fontSize: '0.75rem', fontWeight: 600, color: '#64748b', textTransform: 'uppercase' }}>Tenant</th>
                            </tr>
                        </thead>
                        <tbody>
                            {users.map((u, i) => (
                                <tr key={i} style={{ borderBottom: i === users.length - 1 ? 'none' : '1px solid #e2e8f0' }}>
                                    <td style={{ padding: '1rem 1.5rem', fontSize: '0.875rem', fontWeight: 500 }}>{u.email}</td>
                                    <td style={{ padding: '1rem 1.5rem' }}>
                                        <span style={{
                                            padding: '0.25rem 0.625rem',
                                            borderRadius: '1rem',
                                            fontSize: '0.75rem',
                                            fontWeight: 600,
                                            background: u.role === 'Admin' ? '#fef3c7' : '#dcfce7',
                                            color: u.role === 'Admin' ? '#92400e' : '#166534'
                                        }}>
                                            {u.role}
                                        </span>
                                    </td>
                                    <td style={{ padding: '1rem 1.5rem', fontSize: '0.875rem', color: '#64748b' }}>{u.tenantId}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}
