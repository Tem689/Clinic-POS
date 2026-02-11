'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { apiFetch } from '@/lib/api';

export default function NewPatientPage() {
    const [formData, setFormData] = useState({
        firstName: '',
        lastName: '',
        phoneNumber: '',
        primaryBranchId: '',
    });
    const [loading, setLoading] = useState(false);
    const router = useRouter();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);

        try {
            const res = await apiFetch('/api/patients', {
                method: 'POST',
                body: JSON.stringify({
                    ...formData,
                    primaryBranchId: formData.primaryBranchId ? parseInt(formData.primaryBranchId) : null,
                }),
            });

            if (res.ok) {
                router.push('/patients');
            } else {
                const errData = await res.json();
                alert(errData.message || 'Failed to create patient');
            }
        } catch (err) {
            alert('Network error');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ animation: 'fadeIn 0.5s ease', maxWidth: '640px', margin: '0 auto' }}>
            <div style={{ marginBottom: '2.5rem' }}>
                <button
                    onClick={() => router.back()}
                    style={{
                        background: 'transparent',
                        border: 'none',
                        color: 'var(--text-muted)',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '0.5rem',
                        cursor: 'pointer',
                        padding: 0,
                        fontSize: '0.875rem',
                        fontWeight: 500
                    }}
                >
                    ← Back to directory
                </button>
                <h1 style={{ fontSize: '2rem', fontWeight: 800, color: 'var(--text-main)', letterSpacing: '-0.025em', marginTop: '1rem' }}>Register New Patient</h1>
                <p style={{ color: 'var(--text-muted)' }}>Fill in the medical record details to onboard a new patient</p>
            </div>

            <form onSubmit={handleSubmit} className="card" style={{ padding: '2.5rem', boxShadow: 'var(--shadow-lg)', border: '1px solid var(--border)' }}>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem' }}>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                        <label style={{ fontSize: '0.875rem', fontWeight: 600, color: 'var(--text-main)' }}>First Name</label>
                        <input
                            required
                            placeholder="John"
                            style={{
                                padding: '0.75rem 1rem',
                                borderRadius: '0.75rem',
                                border: '1px solid var(--border)',
                                background: '#f8fafc',
                                fontSize: '0.95rem'
                            }}
                            value={formData.firstName}
                            onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                        />
                    </div>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                        <label style={{ fontSize: '0.875rem', fontWeight: 600, color: 'var(--text-main)' }}>Last Name</label>
                        <input
                            required
                            placeholder="Doe"
                            style={{
                                padding: '0.75rem 1rem',
                                borderRadius: '0.75rem',
                                border: '1px solid var(--border)',
                                background: '#f8fafc',
                                fontSize: '0.95rem'
                            }}
                            value={formData.lastName}
                            onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                        />
                    </div>
                </div>

                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', marginTop: '1.5rem' }}>
                    <label style={{ fontSize: '0.875rem', fontWeight: 600, color: 'var(--text-main)' }}>Phone Number</label>
                    <div style={{ position: 'relative' }}>
                        <span style={{ position: 'absolute', left: '1rem', top: '50%', transform: 'translateY(-50%)', color: '#94a3b8' }}>📞</span>
                        <input
                            required
                            type="tel"
                            placeholder="+66 81 234 5678"
                            style={{
                                width: '100%',
                                padding: '0.75rem 1rem 0.75rem 2.75rem',
                                borderRadius: '0.75rem',
                                border: '1px solid var(--border)',
                                background: '#f8fafc',
                                fontSize: '0.95rem'
                            }}
                            value={formData.phoneNumber}
                            onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                        />
                    </div>
                    <p style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Required for automated appointment reminders</p>
                </div>

                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', marginTop: '1.5rem' }}>
                    <label style={{ fontSize: '0.875rem', fontWeight: 600, color: 'var(--text-main)' }}>Primary Branch ID</label>
                    <select
                        style={{
                            padding: '0.75rem 1rem',
                            borderRadius: '0.75rem',
                            border: '1px solid var(--border)',
                            background: '#f8fafc',
                            fontSize: '0.95rem',
                            appearance: 'none'
                        }}
                        value={formData.primaryBranchId}
                        onChange={(e) => setFormData({ ...formData, primaryBranchId: e.target.value })}
                    >
                        <option value="">Select Branch (Optional)</option>
                        <option value="1">Branch #1 (Siam)</option>
                        <option value="2">Branch #2 (Sukhumvit)</option>
                    </select>
                </div>

                <div style={{ marginTop: '3rem', display: 'flex', gap: '1rem' }}>
                    <button
                        type="submit"
                        disabled={loading}
                        className="btn btn-primary"
                        style={{ flex: 1, height: '50px', fontSize: '1rem' }}
                    >
                        {loading ? 'Processing...' : 'Complete Registration'}
                    </button>
                    <button
                        type="button"
                        onClick={() => router.back()}
                        className="btn btn-outline"
                        style={{ padding: '0 2rem', height: '50px' }}
                    >
                        Cancel
                    </button>
                </div>
            </form>

            <style jsx>{`
                @keyframes fadeIn {
                    from { opacity: 0; transform: translateY(10px); }
                    to { opacity: 1; transform: translateY(0); }
                }
            `}</style>
        </div>
    );
}
