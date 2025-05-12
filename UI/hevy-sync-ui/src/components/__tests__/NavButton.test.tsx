import { render, screen, fireEvent } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { NavButton } from '../NavButton';
import { FitnessCenter as FitnessCenterIcon } from '@mui/icons-material';

const mockNavigate = jest.fn();

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => mockNavigate
}));

describe('NavButton', () => {
  const defaultProps = {
    title: 'Test Button',
    icon: <FitnessCenterIcon />,
    path: '/test',
    description: 'Test Description'
  };

  beforeEach(() => {
    mockNavigate.mockClear();
  });

  it('renders with correct content', () => {
    render(
      <BrowserRouter>
        <NavButton {...defaultProps} />
      </BrowserRouter>
    );

    expect(screen.getByText('Test Button')).toBeInTheDocument();
    expect(screen.getByText('Test Description')).toBeInTheDocument();
  });

  it('navigates to correct path when clicked', () => {
    render(
      <BrowserRouter>
        <NavButton {...defaultProps} />
      </BrowserRouter>
    );

    fireEvent.click(screen.getByText('Test Button'));
    expect(mockNavigate).toHaveBeenCalledWith('/test');
  });

  it('applies hover styles on hover', () => {
    render(
      <BrowserRouter>
        <NavButton {...defaultProps} />
      </BrowserRouter>
    );

    const button = screen.getByText('Test Button').closest('div');
    expect(button).toHaveStyle({
      transition: 'transform 0.2s, box-shadow 0.2s'
    });
  });
}); 