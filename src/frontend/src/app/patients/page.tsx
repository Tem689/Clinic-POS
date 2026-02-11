'use client';

import { useEffect, useState } from 'react';
import { apiFetch } from '@/lib/api';
import Link from 'next/link';

interface Patient {
    id: number;
    firstName: string;
    lastName: string;
    phoneNumber: string;
    createdAt: string;
    primaryBranchId?: number | null;
}

export default function PatientsPage() {
    const [patients, setPatients] = useState<Patient[]>([]);
    const [branchId, setBranchId] = useState<string>('');
    const [loading, setLoading] = useState(true);
    const [user, setUser] = useState<any>(null);

    useEffect(() => {
        const savedUser = localStorage.getItem('clinic_user');
        if (savedUser) setUser(JSON.parse(savedUser));
        fetchPatients();
    }, [branchId]);

    const fetchPatients = async () => {
        setLoading(true);
        try {
            const res = await apiFetch(`/api/patients${branchId ? `?branchId=${branchId}` : ''}`);
            if (res.ok) {
                const data = await res.json();
                setPatients(data);
            }
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const canCreate = user?.role === 'Admin' || user?.role === 'User';

    return (
        <div style={{ animation: 'fadeIn 0.5s ease' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2.5rem' }}>
                <div>
                    <h1 style={{ fontSize: '2rem', fontWeight: 800, color: 'var(--text-main)', letterSpacing: '-0.025em', margin: 0 }}>Patients</h1>
                    <p style={{ color: 'var(--text-muted)', marginTop: '0.25rem' }}>Manage and monitor patient records across branches</p>
                </div>
                {canCreate && (
                    <Link href="/patients/new">
                        <button className="btn btn-primary" style={{ height: '44px', boxShadow: '0 4px 14px 0 rgba(79, 70, 229, 0.39)' }}>
                            <span style={{ fontSize: '1.2rem', fontWeight: 'bold' }}>+</span> Create Patient
                        </button>
                    </Link>
                )}
            </div>

            <div className="card" style={{ marginBottom: '2rem', padding: '1.25rem 1.5rem', display: 'flex', alignItems: 'center', gap: '1rem', background: 'white', borderColor: 'var(--border)' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', flex: 1, position: 'relative' }}>
                    <span style={{ position: 'absolute', left: '1rem', color: '#94a3b8' }}>🔍</span>
                    <input
                        type="number"
                        value={branchId}
                        onChange={(e) => setBranchId(e.target.value)}
                        placeholder="Filter by Branch ID..."
                        style={{
                            width: '100%',
                            padding: '0.75rem 1rem 0.75rem 2.75rem',
                            borderRadius: '0.75rem',
                            border: '1px solid var(--border)',
                            maxWidth: '320px',
                            background: '#f8fafc',
                            fontSize: '0.925rem'
                        }}
                    />
                    {branchId && (
                        <button
                            onClick={() => setBranchId('')}
                            style={{
                                background: 'transparent',
                                border: 'none',
                                color: 'var(--primary)',
                                fontWeight: 600,
                                cursor: 'pointer',
                                fontSize: '0.875rem'
                            }}
                        >
                            Reset
                        </button>
                    )}
                </div>
            </div>

            <div className="card" style={{ border: '1px solid var(--border)', boxShadow: 'var(--shadow)' }}>
                {loading ? (
                    <div style={{ padding: '6rem', textAlign: 'center' }}>
                        <div className="spinner" style={{ marginBottom: '1.5rem' }}></div>
                        <p style={{ color: 'var(--text-muted)', fontWeight: 500 }}>Fetching patient directory...</p>
                    </div>
                ) : (
                    <div style={{ overflowX: 'auto' }}>
                        <table style={{ width: '100%', borderCollapse: 'separate', borderSpacing: 0 }}>
                            <thead>
                                <tr style={{ background: '#f8fafc' }}>
                                    <th style={{ padding: '1rem 1.5rem', textAlign: 'left', fontSize: '0.75rem', fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', borderBottom: '1px solid var(--border)' }}>Patient Profile</th>
                                    <th style={{ padding: '1rem 1.5rem', textAlign: 'left', fontSize: '0.75rem', fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', borderBottom: '1px solid var(--border)' }}>Contact Information</th>
                                    <th style={{ padding: '1rem 1.5rem', textAlign: 'left', fontSize: '0.75rem', fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', borderBottom: '1px solid var(--border)' }}>Branch Assignment</th>
                                    <th style={{ padding: '1rem 1.5rem', textAlign: 'left', fontSize: '0.75rem', fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em', borderBottom: '1px solid var(--border)' }}>Registration Date</th>
                                </tr>
                            </thead>
                            <tbody>
                                {patients.map((p, idx) => (
                                    <tr key={p.id} style={{
                                        transition: 'background 0.2s ease',
                                        borderBottom: idx === patients.length - 1 ? 'none' : '1px solid var(--border)'
                                    }} className="table-row">
                                        <td style={{ padding: '1.25rem 1.5rem' }}>
                                            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                                                <div style={{
                                                    width: '40px',
                                                    height: '40px',
                                                    background: 'var(--primary-light)',
                                                    borderRadius: '10px',
                                                    display: 'flex',
                                                    alignItems: 'center',
                                                    justifyContent: 'center',
                                                    color: 'var(--primary)',
                                                    fontWeight: 700,
                                                    fontSize: '0.875rem'
                                                }}>
                                                    {p.firstName.charAt(0)}{p.lastName.charAt(0)}
                                                </div>
                                                <div>
                                                    <div style={{ fontWeight: 600, color: 'var(--text-main)', fontSize: '0.95rem' }}>{p.firstName} {p.lastName}</div>
                                                    <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>ID: #{p.id}</div>
                                                </div>
                                            </div>
                                        </td>
                                        <td style={{ padding: '1.25rem 1.5rem', color: 'var(--text-main)', fontWeight: 500, fontSize: '0.9rem' }}>
                                            <span style={{ display: 'inline-flex', alignItems: 'center', gap: '0.4rem' }}>
                                                <span style={{ fontSize: '1rem' }}>📞</span> {p.phoneNumber}
                                            </span>
                                        </td>
                                        <td style={{ padding: '1.25rem 1.5rem' }}>
                                            <span style={{
                                                display: 'inline-flex',
                                                alignItems: 'center',
                                                padding: '0.35rem 0.75rem',
                                                background: p.primaryBranchId ? '#ecfdf5' : '#f1f5f9',
                                                color: p.primaryBranchId ? '#059669' : '#64748b',
                                                borderRadius: '2rem',
                                                fontSize: '0.75rem',
                                                fontWeight: 600,
                                                border: p.primaryBranchId ? '1px solid #d1fae5' : '1px solid #e2e8f0'
                                            }}>
                                                {p.primaryBranchId ? `Branch ${p.primaryBranchId}` : 'Unassigned'}
                                            </span>
                                        </td>
                                        <td style={{ padding: '1.25rem 1.5rem' }}>
                                            <div style={{ fontSize: '0.875rem', fontWeight: 500, color: 'var(--text-main)' }}>{new Date(p.createdAt).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}</div>
                                            <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginTop: '0.150rem' }}>{new Date(p.createdAt).toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' })}</div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                        {patients.length === 0 && !loading && (
                            <div style={{ padding: '6rem 2rem', textAlign: 'center' }}>
                                <div style={{
                                    width: '80px',
                                    height: '80px',
                                    background: '#f8fafc',
                                    borderRadius: '50%',
                                    display: 'flex',
                                    alignItems: 'center',
                                    justifyContent: 'center',
                                    fontSize: '2.5rem',
                                    margin: '0 auto 1.5rem'
                                }}>📂</div>
                                <h3 style={{ fontSize: '1.25rem', fontWeight: 700, color: 'var(--text-main)' }}>No patients detected</h3>
                                <p style={{ color: 'var(--text-muted)', marginTop: '0.5rem', maxWidth: '320px', margin: '0.5rem auto 1.5rem' }}>
                                    We couldn't find any records matching your current filter. Try adjusting your search parameters.
                                </p>
                                <button onClick={() => setBranchId('')} className="btn btn-outline">Clear all filters</button>
                            </div>
                        )}
                    </div>
                )}
            </div>

            <style jsx>{`
                .table-row:hover {
                    background-color: #fafafe !important;
                }
                @keyframes fadeIn {
                    from { opacity: 0; transform: translateY(10px); }
                    to { opacity: 1; transform: translateY(0); }
                }
                .spinner {
                    width: 40px;
                    height: 40px;
                    border: 3px solid var(--primary-light);
                    border-top: 3px solid var(--primary);
                    border-radius: 50%;
                    margin: 0 auto;
                    animation: spin 1s linear infinite;
                }
                @keyframes spin {
                    0% { transform: rotate(0deg); }
                    100% { transform: rotate(360deg); }
                }
            `}</style>
        </div>
    );
}
