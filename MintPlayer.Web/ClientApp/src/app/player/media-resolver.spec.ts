import { playlistEntryFromUrl } from './media-resolver';

describe('playlistEntryFromUrl', () => {
  const url = 'https://www.youtube.com/watch?v=fJ9rUzIMcZQ';

  it('defaults key and title to the url', () => {
    const entry = playlistEntryFromUrl(url);
    expect(entry).toEqual({ key: url, url, title: url, routerLink: undefined });
  });

  it('uses an explicit key, title and routerLink when given', () => {
    const entry = playlistEntryFromUrl(url, {
      key: 'Songs/borhap',
      title: 'Bohemian Rhapsody',
      routerLink: ['/po', 'song', 'Songs/borhap'],
    });
    expect(entry).toEqual({
      key: 'Songs/borhap',
      url,
      title: 'Bohemian Rhapsody',
      routerLink: ['/po', 'song', 'Songs/borhap'],
    });
  });

  it('falls back to the url when the title is blank/whitespace', () => {
    expect(playlistEntryFromUrl(url, { title: '   ' }).title).toBe(url);
    expect(playlistEntryFromUrl(url, { title: '' }).title).toBe(url);
  });

  it('trims a provided title', () => {
    expect(playlistEntryFromUrl(url, { title: '  Hello  ' }).title).toBe('Hello');
  });
});
