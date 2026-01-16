import { render, screen } from '@testing-library/react';
import { DocumentStatusBadge } from '../DocumentStatusBadge';

describe('DocumentStatusBadge', () => {
  it('renders pending status correctly', () => {
    render(<DocumentStatusBadge status="Pending" />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveTextContent('Pending');
    expect(badge).toHaveClass('document-status-badge--pending');
  });

  it('renders processing status correctly', () => {
    render(<DocumentStatusBadge status="Processing" />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveTextContent('Processing');
    expect(badge).toHaveClass('document-status-badge--processing');
  });

  it('renders completed status correctly', () => {
    render(<DocumentStatusBadge status="Completed" />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveTextContent('Completed');
    expect(badge).toHaveClass('document-status-badge--completed');
  });

  it('renders failed status correctly', () => {
    render(<DocumentStatusBadge status="Failed" />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveTextContent('Failed');
    expect(badge).toHaveClass('document-status-badge--failed');
  });

  it('renders validation failed status correctly', () => {
    render(<DocumentStatusBadge status="ValidationFailed" />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveTextContent('Validation Failed');
    expect(badge).toHaveClass('document-status-badge--validation-failed');
  });

  it('has correct aria-label', () => {
    render(<DocumentStatusBadge status="Processing" />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveAttribute('aria-label', 'Document status: Processing');
  });
});
