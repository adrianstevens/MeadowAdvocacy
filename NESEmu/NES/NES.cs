public class NES {
    Cartridge cartridge;
    Bus bus;

    public NES() {
        cartridge = new Cartridge(Helper.romPath);
        bus = new Bus(cartridge);

        bus.cpu.Reset();
        
        Console.WriteLine("NES");
    }

    public void Run() {
        int cycles = 0;

        bus.input.UpdateController();

        while (cycles < 29828) {
            int used = bus.cpu.ExecuteInstruction();
            cycles += used;
            bus.ppu.Step(used * 3);
        }

        bus.ppu.DrawFrame(1);
    }
}