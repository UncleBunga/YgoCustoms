-- Slasher - The Shape
local s,id=GetID()
function s.initial_effect(c)
    c:EnableCounterPermit(0x199)
    -- If this card would be destroyed, remove 1 Kill Counter instead
    local e1=Effect.CreateEffect(c)
    e1:SetType(EFFECT_TYPE_CONTINUOUS+EFFECT_TYPE_SINGLE)
    e1:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
    e1:SetCode(EFFECT_DESTROY_REPLACE)
    e1:SetRange(LOCATION_MZONE)
    e1:SetTarget(s.reptg)
    e1:SetOperation(s.repop)
    c:RegisterEffect(e1)

    -- Place 1 Kill Counter on a "Slasher" Spell when destroying a monster by battle
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,0))
    e2:SetCategory(CATEGORY_COUNTER)
    e2:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_TRIGGER_O)
    e2:SetCode(EVENT_BATTLE_DESTROYING)
    e2:SetCondition(s.ctcon)
    e2:SetTarget(s.cttg)
    e2:SetOperation(s.ctop)
    c:RegisterEffect(e2)
end

-- Destruction Replacement Target: Check if there's a counter anywhere (1,1) to remove
function s.reptg(e,tp,eg,ep,ev,re,r,rp,chk)
    local c=e:GetHandler()
    if chk==0 then return c:IsReason(REASON_BATTLE+REASON_EFFECT)
        and not c:IsReason(REASON_REPLACE)
        and Duel.IsCanRemoveCounter(tp,1,1,0x199,1,REASON_EFFECT) end
    if Duel.SelectEffectYesNo(tp,c,96) then -- 96 is the internal engine ID for "Replace?" prompts
        return true
    else return false end
end

-- Destruction Replacement Operation: Actually remove the counter
function s.repop(e,tp,eg,ep,ev,re,r,rp)
    Duel.RemoveCounter(tp,1,1,0x199,1,REASON_EFFECT)
end

-- Counter Placement Condition: Did it destroy an opponent's monster?
function s.ctcon(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    local bc=c:GetBattleTarget()
    return c:IsRelateToBattle() and bc and bc:IsControler(1-tp)
end

-- Filter for face-up "Slasher" cards you control that can hold a Kill Counter
function s.ctfilter(c)
    return c:IsFaceup() and c:IsSetCard(0x991) and c:IsCanAddCounter(0x199,1)
end

function s.cttg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsExistingMatchingCard(s.ctfilter,tp,LOCATION_ONFIELD,0,1,nil) end
    Duel.SetOperationInfo(0,CATEGORY_COUNTER,nil,1,0,0x199)
end

-- Add the Kill Counter (0x199)
function s.ctop(e,tp,eg,ep,ev,re,r,rp)
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_COUNTER)
    local g=Duel.SelectMatchingCard(tp,s.ctfilter,tp,LOCATION_ONFIELD,0,1,1,nil)
    if #g>0 then
        Duel.HintSelection(g)
        g:GetFirst():AddCounter(0x199,1)
    end
end