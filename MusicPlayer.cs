using NAudio.Wave;

namespace ConsolePlayer;

public class MusicPlayer
{
    private AudioFileReader _audioFile;
    private WaveOutEvent _player;
    private List<string> _musicList;
    private int _currentMusicIndex;

    public MusicPlayer()
    {
        _player = new WaveOutEvent();
        _currentMusicIndex = -1;

        _musicList = Directory.GetFiles("audio").ToList();
    }
    public void Control()
    {
        while(true)
        {

            Console.Clear();

            if (_player.PlaybackState == PlaybackState.Playing)
                PrintSong("Now Playing ", _musicList[_currentMusicIndex], ConsoleColor.Blue);
            else if (_player.PlaybackState == PlaybackState.Paused)
                PrintSong("Paused ", _musicList[_currentMusicIndex], ConsoleColor.Blue);

            Console.WriteLine();
            PrintWithMark("-> ", "Play next", ConsoleColor.Blue);
            PrintWithMark("<- ", "Play previous", ConsoleColor.DarkBlue);
            Console.WriteLine();
            PrintWithMark("C ", "Play song by id", ConsoleColor.Gray);
            PrintWithMark("V ", "Change volume", ConsoleColor.DarkGray);
            PrintWithMark("P ", "Pause/Play song", ConsoleColor.DarkGreen);
            PrintWithMark("S ", "Seek to time", ConsoleColor.DarkYellow);
            PrintWithMark("A ", "Add song by path", ConsoleColor.Cyan);

            Console.WriteLine();
            ConsoleKey input = Console.ReadKey().Key;
            //Удаление последней строки!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Console.SetCursorPosition(0, Console.CursorTop);

            if (input == ConsoleKey.RightArrow)
                PlayNext();
            if (input == ConsoleKey.LeftArrow)
                PlayPrevious();
            if (input == ConsoleKey.C)
            {
                int id = 1;
                SongList();
                PrintWithMark("- ", "Waiting for song id: \n", ConsoleColor.DarkRed);
                try { id = Int32.Parse(Console.ReadLine()); Console.SetCursorPosition(0, Console.CursorTop - 1); } catch (Exception ex) {PrintWithMark("- ", "Wrong number choosed first song automaticaly", ConsoleColor.DarkRed); Thread.Sleep(1500); }
                ChooseSongById(id);
            }    
            if (input == ConsoleKey.V)
            {
                PrintWithMark("- ", "Write new volume (0-100): ", ConsoleColor.DarkRed);
                try
                {
                    int volume = Int32.Parse(Console.ReadLine());
                    ChangeVolume(volume);
                }
                catch (Exception ex)
                {
                    PrintWithMark("- ", "Wrong number volume not changed", ConsoleColor.DarkRed);
                    Thread.Sleep(1500);
                }
            }
            if (input == ConsoleKey.P)
                Pause();
            if (input == ConsoleKey.S)
            {
                PrintWithMark("- ", "Enter time to seek to (in seconds): ", ConsoleColor.DarkRed);
                try
                {
                    int seconds = Int32.Parse(Console.ReadLine());
                    Seek(seconds);
                }
                catch (Exception ex)
                {
                    PrintWithMark("- ", "Invalid time entered", ConsoleColor.DarkRed);
                    Thread.Sleep(1500);
                }
            }
            if (input == ConsoleKey.A)
            {
                PrintWithMark("- ", "Enter full path to the music file: ", ConsoleColor.Cyan);
                try
                {
                    string filePath = Console.ReadLine();
                    AddSongByPath(filePath);
                }
                catch (Exception ex)
                {
                    PrintWithMark("- ", "Invalid file path", ConsoleColor.DarkRed);
                    Thread.Sleep(1500);
                }
            }

        }
    }
 
    public void Play(int audioFileIndex)
    {
        _currentMusicIndex = audioFileIndex;

        if(_player.PlaybackState==PlaybackState.Playing)       
            _player.Stop();
        
        _audioFile = new AudioFileReader(_musicList[audioFileIndex]);

        _player.Init(_audioFile);
        _player.Play();
    }
    public void Pause()
    {
        if (_player.PlaybackState == PlaybackState.Playing)
        {
            _player.Pause();
            PrintWithMark("- ", "Song paused.", ConsoleColor.DarkMagenta);
        }
        else
        {
            if (_player.PlaybackState == PlaybackState.Paused)
            {
                _player.Play();
                PrintWithMark("- ", "Song unpaused.", ConsoleColor.DarkMagenta);
            }
            else
            {
                if (_player.PlaybackState == PlaybackState.Stopped)
                {
                    PrintWithMark("- ", "Theres no song that played at the moment.", ConsoleColor.DarkMagenta);
                }
            }
        }
        Thread.Sleep(1500);
    }
    public void PlayNext()
    {
        _currentMusicIndex++;
        if (_currentMusicIndex >= _musicList.Count)
            _currentMusicIndex = 0;
        Play(_currentMusicIndex);
    }
    public void PlayPrevious()
    {
        _currentMusicIndex--;
        if (_currentMusicIndex < 0)
            _currentMusicIndex = _musicList.Count-1;
        Play(_currentMusicIndex);
    }

    public void SongList()
    {
        int i = 0;
        foreach (string song in _musicList)
        { i++; Console.WriteLine($"{i}. {song}"); }
    }
    public void ChooseSongById(int id)
    {
        id--;
        try {
            if (id > _musicList.Count || id <= -1) { Console.WriteLine($"{id + 1} not valid id of the song, try again later!"); Thread.Sleep(1500); } 
            else Play(id);
        }
        catch (Exception ex) { Console.WriteLine($"{id + 1} not valid id of the song, try again later!"); Thread.Sleep(1500); }
        
    }

    public void ChangeVolume(int volume)
    {
        _player.Volume = ((float)volume / 100);
        Console.SetCursorPosition(0, Console.CursorTop-1);
        PrintWithMark("-> ", $"Volume set to {volume}", ConsoleColor.DarkBlue);
        Thread.Sleep(1500);
    }
    public void Seek(int seconds)
    {
        if (_audioFile != null)
        {
            long position = seconds * _audioFile.WaveFormat.AverageBytesPerSecond;
            if (position < 0)
            {
                position = 0;
            }
            if (position > _audioFile.Length)
            {
                position = _audioFile.Length;
            }
            _audioFile.Position = position;
            PrintWithMark("-> ", $"Seeked to {seconds} seconds", ConsoleColor.DarkYellow);
        }
        else
        {
            PrintWithMark("-> ", "No song is currently loaded", ConsoleColor.DarkRed);
        }
        Thread.Sleep(1500);
    }
    public void AddSongByPath(string filePath)
    {
        if (File.Exists(filePath))
        {
            string fileName = Path.GetFileName(filePath);
            string destFilePath = Path.Combine("audio" , fileName);

            if (!File.Exists(destFilePath))
            {
                File.Copy(filePath, destFilePath);
                _musicList.Add(destFilePath);
                PrintWithMark("-> ", $"File copied and added: {filePath}", ConsoleColor.Cyan);
            }
            else
            {
                PrintWithMark("-> ", "File already exists in the target directory", ConsoleColor.DarkRed);
            }
        }
        else
        {
            PrintWithMark("-> ", "File does not exist", ConsoleColor.DarkRed);
        }
        Thread.Sleep(1500);
    }
    private void PrintWithMark(string mark, string content, ConsoleColor color)
    {
        Console.ForegroundColor= color;
        Console.Write(mark);
        Console.ResetColor();
        Console.WriteLine(content);

    }

    private void PrintSong(string start, string song, ConsoleColor color)
    {
       
        Console.Write(start);
        Console.ForegroundColor = color;
        Console.WriteLine(song);
        Console.ResetColor();
    }
}
